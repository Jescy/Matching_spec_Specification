//-----------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
//-----------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.SpecSharp;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using ssc;
#if CCINamespace
using Microsoft.Cci;
#else
using System.Compiler;
#endif

namespace IncrementalCompilationTests
{

    public class Test
    {
        private  const int EXACT_MATCH = 0;
        private  const int PLUG_IN_MATCH = 1;
        private  const int PLUG_IN_POST_MATCH = 2;
        private  const int WEAK_POST_MATCH = 3;


        StreamReader reader;
        string inputLine;
        string testName;
        int lineCounter = 0;
        int failures = 0;
        Compilation compilation;
        CompilationList compilations;
        CompilationUnit compilationUnit;
        System.Compiler.Compiler compiler;
        ErrorNodeList errors;
        StringBuilder output;
        TextWriter savedConsoleOut;
        System.Diagnostics.TextWriterTraceListener traceListener;
        Thread currentThread;
        Document currentDocument;


        static int Main(string[] args)
        {
            string[] arg = new string[] { "/t:library", "/debug", "/nn", "G:\\\\test1.txt", "G:\\\\test2.txt", "G:\\\\MyObject.txt" };
            CompilerMain.Main(arg);


            
            Compilation mCompilation = Microsoft.SpecSharp.Compiler.myCompilation;
                 
            
            MyVisitor sv1 = new MyVisitor();
            MyVisitor sv2 = new MyVisitor();

            CompilationUnit cu1 = mCompilation.CompilationUnits[0];
            CompilationUnit cu2 = mCompilation.CompilationUnits[1];
            sv1.Visit(cu1);
            sv2.Visit(cu2);


            // to see whether the signature type match    
            if (isSameReType(sv1.getSty(),sv2.getSty()))
            {
                ArrayList test1 = sv1.getParaType();
                ArrayList test2 = sv2.getParaType();


                if (isSameParaType(test1, test2))
                {
                    //signature is total match, go into the step of rename
                    VisitorForRename V = new VisitorForRename(sv1.getParaName(), sv2.getParaName());

                    // using AST2 to invoke V, then the rename process can work. However its only works in AST
                    cu2=(CompilationUnit)V.Visit(cu2);

                    //using printer to print out the specification part
                    OutputVisitor ou1 = new OutputVisitor();
                    ou1.start(cu1);
                    ArrayList ens1 = ou1.ensures;
                    ArrayList req1 = ou1.requires;

                    OutputVisitor ou2 = new OutputVisitor();
                    ou2.start(cu2);
                    ArrayList ens2 = ou2.ensures;
                    ArrayList req2 = ou2.requires;

                    string strexact = combine(EXACT_MATCH, req1, ens1, req2, ens2);
                   // Console.Out.WriteLine(strexact);
                    string strplm = combine(PLUG_IN_MATCH, req1, ens1, req2, ens2);
                   // Console.Out.WriteLine(strplm);
                    string strplpm = combine(PLUG_IN_POST_MATCH, req1, ens1, req2, ens2);
                   // Console.Out.WriteLine(strplpm);
                    string strwpm = combine(WEAK_POST_MATCH, req1, ens1, req2, ens2);
                    //Console.Out.WriteLine(strwpm);
                   
                    //using file operator technique to print out the specification part 
                    //                                              File.WriteAllText("G:\\test3.txt", "");
                    //                                              OutputContractVisitor ocv = new OutputContractVisitor(test1, test2);
                    //                                              ocv.Visit(cu1);
                    //                                              ocv.Visit(cu2);
                    //                    modifyTextRegExp(sv1.getParaName(), sv2.getParaName(), test1.Count);
                    // 
                    //                    getFinalText();

                    string strTest = File.ReadAllText("G:\\test1.txt");
                    modifiedStr(strTest, strplm); //here we use Plug-in match as an example for output
                }


            }

            else
            {

                Console.Out.WriteLine("signature type do not match");

            }

            Console.Read();
            return 0;
        }
        static bool isSameReType(TypeNode tta, TypeNode ttb)
        {
            HashedMap mHashMap = Microsoft.SpecSharp.Looker.mHashedMap;
            TypeNode ta = mHashMap.Contains(tta.ToString()) ? (TypeNode)mHashMap[tta.ToString()] : tta;
            TypeNode tb = mHashMap.Contains(ttb.ToString()) ? (TypeNode)mHashMap[ttb.ToString()] : ttb;
            
            if (ta.Equals(tb))
                return true;
            if (ta.IsDerivedFrom(tb) || tb.IsDerivedFrom(ta))
                return true;
            return false;
        }
        static string getFinalText()
        {
            string[] code = File.ReadAllLines(@"g:\\test3.txt");
            ArrayList ens = new ArrayList();
            ArrayList res = new ArrayList();
            foreach (string str in code)
            {
                if (str.StartsWith("ensures"))
                    ens.Add(str.Substring(8));
                else if (str.StartsWith("requires"))
                    ens.Add(str.Substring(9));
            }
            return "";
        }
        
        static void modifyTextRegExp(ArrayList a, ArrayList b, int number)
        {
            string code = File.ReadAllText(@"g:\\test3.txt");

            for (int i = 0; i < number; i++)
            {
                string name1 = (string)(a[i]);
                string name2 = (string)(b[i]);
                string reg = @"(?<=\W)" + name1 + @"(?=\W)";
                code = Regex.Replace(code, reg, name2, RegexOptions.Multiline);
                


            }
            File.WriteAllText(@"g:\\test3.txt", code);
        }


        static bool isSameParaType(ArrayList a, ArrayList b)
        {
            if (a.Count != b.Count)
            {
                Console.Out.WriteLine("parameter number don't match");
                return false;
            }
            for (int i = 0; i < a.Count; i++)
            {
                TypeNode ta = (TypeNode)a[i];
                TypeNode tb = (TypeNode)b[i];
                if (!ta.GetType().Equals(tb.GetType()))
                    return false;
                if (ta.GetType().Equals(typeof(NonNullableTypeExpression)))
                    ta = ((NonNullableTypeExpression)ta).ElementType;

                if (tb.GetType().Equals(typeof(NonNullableTypeExpression)))
                    tb = ((NonNullableTypeExpression)tb).ElementType;

                if (ta.GetType().Equals(typeof(ArrayTypeExpression)))
                {
                    string expA = ((TypeExpression)(ta.StructuralElementTypes[0])).Expression.ToString();
                    string expB = ((TypeExpression)(tb.StructuralElementTypes[0])).Expression.ToString();
                    if (expA != expB)
                    {
                        Console.Out.WriteLine("the parameter array type does not match");
                        return false;
                    }
                }
                else
                {
                    string expA = ((TypeExpression)ta).Expression.ToString();
                    string expB = ((TypeExpression)tb).Expression.ToString();
                    if (expA != expB)
                    {
                        Console.Out.WriteLine("parameter type does not match");
                        return false;
                    }
                }

            }
            return true;
        }
        private static string connect(ArrayList array)
        {
            StringBuilder res = new StringBuilder();
            res.Append("(");
            for (int i = 0; i < array.Count; i++)
            {
                if (i > 0)
                    res.Append("&&");
                res.Append("(" + array[i] + ")");
            }
            res.Append(")");
            return res.ToString();
        }
        private static string combine(int mode, ArrayList req1, ArrayList ens1, ArrayList req2, ArrayList ens2)
        {
            StringBuilder result = new StringBuilder();
            string reqs1, reqs2, enss1, enss2;
            reqs1 = connect(req1);
            reqs2 = connect(req2);
            enss1 = connect(ens1);
            enss2 = connect(ens2);

            switch (mode)
            {
                case EXACT_MATCH:
                    result.Append("ensures");
                    result.Append("(");
                    result.Append(reqs2);
                    result.Append("<==>");
                    result.Append(reqs1);
                    result.Append(")");

                    result.Append("&&");

                    result.Append("(");
                    result.Append(enss1);
                    result.Append("<==>");
                    result.Append(enss2);
                    result.Append(")");
                    result.Append(";");
                    result.Append("\r\n");
                    break;
                case PLUG_IN_MATCH:
                    result.Append("ensures");
                    result.Append("(");
                    result.Append(reqs2);
                    result.Append("==>");
                    result.Append(reqs1);
                    result.Append(")");

                    result.Append("&&");

                    result.Append("(");
                    result.Append(enss1);
                    result.Append("==>");
                    result.Append(enss2);
                    result.Append(")");
                    result.Append(";");
                    result.Append("\r\n");
                    break;
                case PLUG_IN_POST_MATCH:
                    result.Append("ensures");
                    result.Append("(");
                    result.Append(enss1);
                    result.Append("<==>");
                    result.Append(enss2);
                    result.Append(")");
                    result.Append("\r\n");
                    break;
                case WEAK_POST_MATCH:
                    result.Append(reqs1);
                  
                    result.Append("==>");

                    result.Append("(");
                    result.Append(enss1);
                    result.Append("<==>");
                    result.Append(enss2);
                    result.Append(")");
                    result.Append(";");
                    result.Append("\r\n");
                    break;


            }
            return result.ToString();
        }
        static string modifiedStr(string input,string replace)
        {
            string reg = @"((requires)|(ensures))[\s\S]+?;[\s]*?(?=\{)";
            string output = Regex.Replace(input, reg, replace, RegexOptions.Singleline);
            output = Regex.Replace(output, @"invariant", @"//invariant", RegexOptions.Multiline);
            Console.Out.Write(output);
            return output;
        }

    }
}