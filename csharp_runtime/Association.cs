using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace sccdlib
{
    /**
     * wrapper object for one association relation
     */
    public class Association
    {
        string name;
        string class_name;
        int min_card;
        int max_card;
        int next_id = 0;
        Dictionary<int, InstanceWrapper> instances = new Dictionary<int ,InstanceWrapper>();
        Dictionary<InstanceWrapper, int> instances_to_id = new Dictionary<InstanceWrapper, int>();
        
        public Association (string name, string class_name, int min_card, int max_card)
        {
            this.min_card = min_card;
            this.max_card = max_card;
            this.name = name;
            this.class_name = class_name;
        }
        
        public string getName ()
        {
            return this.name;
        }
        
        public string getClassName ()
        {
            return this.class_name;   
        }
        
        public bool allowedToAdd ()
        {
            return (this.max_card == -1) || ( this.instances.Count < this.max_card );    
        }

        public bool allowedToRemove ()
        {
            return (this.instances.Count > this.min_card);    
        }
        
        public int addInstance (InstanceWrapper instance)
        {
            if (this.allowedToAdd ()) {
                this.instances[this.next_id] = instance;
                this.instances_to_id[instance] = this.next_id;
                this.next_id++;
            } else {
                throw new AssociationException("Not allowed to add the instance to the association.");
            }
            return this.next_id - 1;
        }

        public void removeInstance(InstanceWrapper instance)
        {
            if (this.allowedToRemove())
            {
                this.instances.Remove(this.instances_to_id [instance]);
                this.instances_to_id.Remove(instance);
            }else
            {
                throw new AssociationException("Not allowed to remove the instance from the association.");
            }
        }

        public void removeInstance(int instance_id)
        {
            if (this.allowedToRemove())
            {
                this.instances_to_id.Remove(this.instances[instance_id]);
                this.instances.Remove(instance_id);
            }else
            {
                throw new AssociationException("Not allowed to remove the instance from the association.");
            }
        }

        public void removeAllInstances()
        {
            if (this.min_card == 0)
            {
                this.instances_to_id.Clear();
                this.instances.Clear();
            } else
            {
                throw new AssociationException("Not allowed to remove all instances from the association as the minimum cardinality is not 0.");
            }
        }
        
        public IEnumerable<InstanceWrapper> getAllInstances ()
        {
            return this.instances_to_id.Keys;
        }
        
        public InstanceWrapper getInstance(int id)
        {
            try 
            {
                return this.instances[id];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new AssociationException("Invalid index for fetching instance from association.");
            }
        }
        
    }
}

