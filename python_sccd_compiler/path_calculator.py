from visitor import Visitor

class PathCalculator(Visitor):
    """ Computes the states that must be exited and entered for a specific transition if the system is to make
        that transition. 
    """
    
    def visit_ClassDiagram(self, class_diagram): 
        for c in class_diagram.classes :
            c.accept(self)

    def visit_Class(self, c):
        c.statechart.accept(self)
        
    def visit_StateChart(self, statechart):
        for node in statechart.basics + statechart.composites:
            node.accept(self)
                     
    def visit_StateChartNode(self, node):
        for transition in node.transitions :
            transition.accept(self)
            
    def visit_StateChartTransition(self, transition):
        
        source_node = transition.getParentNode()
        target_nodes = transition.getTargetNodes()
        #Find the scope of the transition (lowest common proper ancestor)
        #TODO: Could it be made more efficient if we calculated the LCA from the source node and just one of the target nodes?

        LCA = self.getLCA([source_node] + target_nodes)
        
        source_ancestors = source_node.getAncestors()
        transition.exit_nodes = source_ancestors[:source_ancestors.index(LCA)]
        
        transition.enter_nodes = []
        
        #we then add the branching paths to the other nodes
        for target_node in target_nodes :
            to_append = []
            for (ancestor_index,ancestor) in enumerate(target_node.getAncestors()) :
                add = ancestor != LCA; #If we reach the LCA in the ancestor hierarchy we don't add and break
                for enter_node_entry in transition.enter_nodes :
                    if ancestor == enter_node_entry[0] :
                        add = False #If we reach an ancestor in the hierarchy that is already listed as enter node, we don't add and break
                        break
                if add:
                    to_append.append( (ancestor, ancestor_index==0 ) ) #Only the first from the ancestor list should get True
                else :
                    break
                    
            to_append.reverse() #the enter sequence should be in the reverse order of the ancestor hierarchy
            transition.enter_nodes.extend(to_append)       

    def getLCA(self, nodes):
        """
        Calculates the lowest common ancestor of the nodes
        """ 
        x = nodes.pop()
        for anc in x.getAncestors() :
            all_descendants = True 
            for node in nodes :
                if not node.isDescendantOf(anc) :
                    all_descendants = False
                    break
            if all_descendants :
                return anc