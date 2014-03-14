using System;
using System.Collections.Generic;

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
        List<InstanceWrapper> instances;
        
        public Association (string name, string class_name, int min_card, int max_card)
        {
            this.min_card = min_card;
            this.max_card = max_card;
            this.name = name;
            this.class_name = class_name;
            this.instances = new List<InstanceWrapper>();
        }
        
        public string getName ()
        {
            return this.name;
        }
        
        private bool allowedToAdd ()
        {
            return ( (this.max_card == -1) || ( this.instances.Count < this.max_card ) );    
        }
        
        public void addInstance (InstanceWrapper instance)
        {
            if (this.allowedToAdd ()) {
                this.instances.Add (instance);
            } else {
                throw new AssociationException("Not allowed to add the instance to the association.");
            }
        }
        
        public IList<InstanceWrapper> getAllInstances ()
        {
            return this.instances.AsReadOnly ();
        }
        
        public InstanceWrapper getInstance(int index)
        {
            try 
            {
                return this.instances[index];
            }
            catch (ArgumentOutOfRangeException Exception)
            {
                throw new AssociationException("Invalid index for fetching instance from association.");
            }
        }
        
    }
}

