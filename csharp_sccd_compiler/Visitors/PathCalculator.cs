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

        public override void visit(ClassDiagram class_diagram)
        {
            foreach(Class c in class_diagram.classes)
                c.accept(this);
        }

        public override void visit(Class c)
        {
            c.statechart.accept(this);
        }

        public override void visit(StateChart statechart)
        {
            foreach (StateChartNode node in statechart.basics.Concat(statechart.composites))
                node.accept(this);
        }

        public override void visit(StateChartNode node)
        {
            foreach (StateChartTransition transition in node.transitions)
                transition.accept(this);
        }

        public override void visit(StateChartTransition transition)
        {
            //Find the scope of the transition (lowest common proper ancestor)
            //TODO: Could it be made more efficient if we calculated the LCA from the source node and just one of the target nodes?

            StateChartNode LCA = this.calculateLCA(transition);
            //Calculate exit nodes
            transition.exit_nodes = new List<StateChartNode>(){transition.parent};
            foreach(StateChartNode node in transition.parent.getAncestors())
            {
                if (object.ReferenceEquals(node, LCA))
                    break;
                transition.exit_nodes.Add(node);
            }
            //Calculate enter nodes
            transition.enter_nodes = new List<Tuple<StateChartNode, bool>>();

            foreach (StateChartNode target_node in transition.target.target_nodes)
            {
                var to_append = new List<Tuple<StateChartNode, bool>>(){new Tuple<StateChartNode, bool>(target_node, true)};
                foreach (StateChartNode anc in target_node.getAncestors())
                {
                    if (object.ReferenceEquals(anc, LCA))//If we reach the LCA in the ancestor hierarchy we break
                        break;
                    bool to_add = true; //boolean value to see if the current ancestor should be added to the result
                    foreach (Tuple<StateChartNode, bool> enter_node_entry in transition.enter_nodes)
                    {
                        if (object.ReferenceEquals(enter_node_entry.Item1, anc))
                        {
                            to_add = false; //If we reach an ancestor in the hierarchy that is already listed as enter node, we don't add and break
                            break;
                        }
                    }
                    if (to_add)
                        to_append.Add(new Tuple<StateChartNode, bool>(anc, false)); //Only target nodes get true
                    else
                        break;
                }
                to_append.Reverse();
                transition.enter_nodes.AddRange(to_append);
            }
        }

        private StateChartNode calculateLCA(StateChartTransition transition)
        {
            foreach(StateChartNode anc in transition.parent.getAncestors())
            {
                bool all_descendants = true;
                foreach (StateChartNode node in transition.target.target_nodes)
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

