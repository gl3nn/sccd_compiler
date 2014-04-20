using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class InstanceWrapper
    {
        RuntimeClassBase instance;
        Dictionary<string,Association> associations = new Dictionary<string, Association>();
        
        
        public InstanceWrapper (RuntimeClassBase instance, List<Association> associations)
        {
            this.instance = instance;
            foreach (var association in associations) {
                this.associations[association.getName()] = association;   
            }
        }
        
        public Association getAssociation (string name)
        {
            try{
                return this.associations[name];
            }catch (KeyNotFoundException) {
                throw new AssociationReferenceException("Unknown association.");
            }
        }
        
        public RuntimeClassBase getInstance ()
        {
            return this.instance;
        }
    }
}