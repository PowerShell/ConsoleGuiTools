
using System;
using System.Management.Automation;

namespace OutGridView.Cmdlet.TreeNodeCaching
{
    class PsoHelper
    {
        internal static bool IsDisplayableMember(PSMemberInfo m)
        {
            if(!m.IsInstance)
            {
                return false;
            }
            

            if(
                m.MemberType == PSMemberTypes.Method ||
                m.MemberType == PSMemberTypes.CodeMethod ||
                m.MemberType == PSMemberTypes.Event ||
                m.MemberType == PSMemberTypes.Methods
                ){
                    
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unwraps the <see cref="PSObject.BaseObject"/> if it is likely to
        /// be a better (e.g. native) representation of the users input.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>The native object or the original reference if unwrapping is counter productive.</returns>
        internal static object MaybeUnwrap(PSObject obj)
        {
            if(ShouldUnwrap(obj))
            {
                return Unwrap(obj);
            }
            
            return obj;
        }

        private static bool ShouldUnwrap(PSObject obj)
        {  
            if(obj.BaseObject is PSCustomObject)
            {
                return false;
            }

            return true;
        }

        private static object Unwrap(PSObject psoVal)
        {
            return psoVal.BaseObject;
        }
    }
}