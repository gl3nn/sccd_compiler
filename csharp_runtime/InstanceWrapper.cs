using System;
using System.Collections.Generic;

namespace sccdlib
{
    public class InstanceWrapper
    {
        IRuntimeClass instance;
        Dictionary<string,Association> associations = new Dictionary<string, Association>();
        
        
        public InstanceWrapper (IRuntimeClass instance, List<Association> associations)
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
                throw new AssociationReferenceException(String.Format("Unknown association '{0}'.", name));
            }
        }
        
        public IRuntimeClass getInstance ()
        {
            return this.instance;
        }
    }
}