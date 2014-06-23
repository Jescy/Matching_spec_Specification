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
    //file operator to deal with the print out problem
    public class OutputContractVisitor : StandardVisitor
    {
        ArrayList a, b;
        static string contractRename(Method meth, ArrayList a, ArrayList b)
        {
            MethodContract mc = meth.Contract;

            File.AppendAllText("g:\\test3.txt", meth.HelpText + "\r\n");
            return "";

        }
        public OutputContractVisitor(ArrayList a, ArrayList b)
        {
            this.a = a;
            this.b = b;
        }
        public override Method VisitMethod(Method method)
        {
            contractRename(method, a, b);
            return base.VisitMethod(method);
        }
    }

}
