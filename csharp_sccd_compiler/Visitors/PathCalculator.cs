using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace csharp_sccd_compiler
{

    /// <summary>
    /// Computes the states that must be exited and entered for a specific transition if the system is to make that transition. 
    /// </summary>
    public class PathCalculator : Visitor
    {
        public PathCalculator()
        {
        }

        public void visit(ClassDiagram class_diagram)
        {
            foreach(Class c in class_diagram.classes)
                c.accept(this);
        }

        public void visit(Class c)
        {
            c.statechart.accept(this);
        }

        public void visit(StateChart statechart)
        {
            foreach (StateChartNode node in statechart.basics.Concat(statechart.composites))
                node.accept(this);
        }

        public void visit(StateChartNode node)
        {
            foreach (StateChartTransition transition in node.transitions)
                transition.accept(this);
        }

        public void visit(StateChartTransition transition)
        {
            StateChartNode source_node = transition.parent;
            List<StateChartNode> target_nodes = transition.target.target_nodes;

            //Find the scope of the transition (lowest common proper ancestor)
            //TODO: Could it be made more efficient if we calculated the LCA from the source node and just one of the target nodes?

            StateChartNode LCA = this.calculateLCA(source_node, target_nodes);

            //Calculate exit nodes
            List<StateChartNode> source_ancestors = source_node.getAncestors();
            int index = 0;
            while (!object.ReferenceEquals(source_ancestors[index],LCA))
                index++;
            transition.exit_nodes = source_ancestors.GetRange(0,index);

            //we first calculate the enter path as if there is only one target node
            var first_target_ancestors = target_nodes[0].getAncestors();
            index = 0;
            while (!object.ReferenceEquals(first_target_ancestors[index],LCA))
                index++;
            first_target_ancestors = first_target_ancestors.GetRange(0,index);

            transition.enter_nodes = new List<Tuple<StateChartNode, bool>>();

            foreach (StateChartNode target_node in target_nodes)
            {
                var to_append = new List<Tuple<StateChartNode, bool>>();
                var ancestors = target_node.getAncestors();
                for (int ancestor_index = 0; ancestor_index < ancestors.Count; ++ancestor_index)
                {
                    bool to_add = object.ReferenceEquals(LCA, ancestors[ancestor_index]); //If we reach the LCA in the ancestor hierarchy we don't add and break
                    foreach (Tuple<StateChartNode, bool> enter_node_entry in transition.enter_nodes)
                    {
                        if (object.ReferenceEquals(enter_node_entry.Item1, ancestors[ancestor_index]))
                        {
                            to_add = false; //If we reach an ancestor in the hierarchy that is already listed as enter node, we don't add and break
                            break;
                        }
                    }
                    if (to_add)
                        to_append.Add(new Tuple<StateChartNode, bool>(ancestors[ancestor_index], ancestor_index == 0)); //Only the first from the ancestor list should get True
                    else
                        break;
                }
                to_append.Reverse();
                transition.enter_nodes.AddRange(to_append);
            }
        }

        private StateChartNode calculateLCA(StateChartNode source_node, List<StateChartNode> target_nodes)
        {
            foreach(StateChartNode anc in source_node.getAncestors())
            {
                bool all_descendants = true;
                foreach (StateChartNode node in target_nodes)
                {
                    if (!node.isDescendantOf(anc))
                    {
                        all_descendants = false;
                        break;
                    }
                }
                if (all_descendants)
                    return anc;
            }
            return null;
        }
    }
}

