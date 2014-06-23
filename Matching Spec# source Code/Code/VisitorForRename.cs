using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.SpecSharp;
using System.IO;
using System.Text;
using System.Collections;
#if CCINamespace
using Microsoft.Cci;
#else
using System.Compiler;
#endif

namespace IncrementalCompilationTests
{
    //rename, prepare for specification matching
    public class VisitorForRename : StandardVisitor
    {
        private ArrayList V1;
        private ArrayList V2;

        public VisitorForRename(ArrayList a, ArrayList b)
        {

            V1 = a;
            V2 = b;

        }

        public override Expression VisitNameBinding(NameBinding nameBinding)
        {
            for (int i = 0; i < V2.Count; i++)
            {
                if (nameBinding.Identifier.Name.Equals((string)V2[i]))
                {
                    nameBinding.Identifier.Name = (string)V1[i];
                    break;
                }
            }
            return nameBinding;
        }
        private Expression renameParam(Identifier id)
        {
            for (int i = 0; i < V2.Count; i++)
            {
                if (id.Name.Equals((string)V2[i]))
                {
                    id.Name = (string)V1[i];
                    break;
                }
            }
            return id;
        }
        public override Expression VisitParameter(Parameter parameter)
        {
            parameter.Name = (Identifier)renameParam(parameter.Name);
            return base.VisitParameter(parameter);
        }

    }
}
