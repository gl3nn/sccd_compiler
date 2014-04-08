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


        #we first calculate the enter path as if there is only one target node
        first_target_ancestors = target_nodes[0].getAncestors()
        first_target_ancestors = first_target_ancestors[:first_target_ancestors.index(LCA)]
        
        transition.enter_nodes = [(first_target_ancestors[0],True)]        
        transition.enter_nodes.extend([(node, False) for node in first_target_ancestors[1:]])
        
        transition.enter_nodes.reverse()
        
        
        #we then add the branching paths to the other nodes
        for target_node in target_nodes[1:] :
            to_append = []
            for (i,ancestor) in enumerate(target_node.getAncestors()) :
                if ancestor not in transition.enter_nodes :
                    to_append.append( (ancestor, i==0 ) ) #Only the first from the ancestor list should get True
                else :
                    break
                    
            to_append.reverse()
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