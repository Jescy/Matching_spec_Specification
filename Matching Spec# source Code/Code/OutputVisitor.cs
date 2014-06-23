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
    //print out visitor
    public class OutputVisitor : StandardVisitor
    {
        private StringBuilder output=new StringBuilder();
        public ArrayList ensures = new ArrayList();
        public ArrayList requires = new ArrayList();

        public void start(Node node)
        {
            ensures.Clear();
            requires.Clear();
            Visit(node);
        }

        public override EnsuresNormal VisitEnsuresNormal(EnsuresNormal normal)
        {
            
            string str=CodePrinter.NodeToString(normal.PostCondition);
            ensures.Add(str);
            return normal;
        }
        public override RequiresPlain VisitRequiresPlain(RequiresPlain plain)
        {
            string str = CodePrinter.NodeToString(plain.Condition);
            requires.Add(str);
            return plain;
        }
    }
}
