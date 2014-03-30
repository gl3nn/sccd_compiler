using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class InstanceWrapper
    {
        RuntimeClassBase instance;
        Dictionary<string,Association> associations = new Dictionary<string, Association>();
        
        
        public InstanceWrapper (RuntimeClassBase instance)
        {
            this.instance = instance;
        }
        
        public void addAssociation (Association association)
        {
            this.associations[association.getName()] = association;
        }
        
        public Association getAssociation (string name)
        {
            try{
                return this.associations[name];
            }catch (Exception e) {
                throw new AssociationReferenceException("Unknown association.");
            }
        }
        
        public RuntimeClassBase getInstance ()
        {
            return this.instance;
        }
    }
}