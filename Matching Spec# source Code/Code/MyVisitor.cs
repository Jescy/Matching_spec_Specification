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
   //get parameter list
    public class MyVisitor : StandardVisitor
    {

        //get parameter name, prepare for signature matching 
        public ArrayList getParaName()
        {
            return parameterName;
        }
        public ArrayList parameterName = new ArrayList();
        public override ParameterList VisitParameterList(ParameterList parameterList)
        {
            ParameterList re = base.VisitParameterList(parameterList);
            if (parameterList == null)
            {
                return parameterList;
            }
            for (int i = 0, n = parameterList.Count; i < n; i++)
            {
                Parameter tmp = parameterList[i];
                parameterName.Add(tmp.Name.Name);
            }

            return re;
        }


        //get parameter type, prepare for signature matching 
        public ArrayList getParaType()
        {
            return parameterType;
        }


        public ArrayList parameterType = new ArrayList();
        public override Expression VisitParameter(Parameter parameter)
        {
            Expression re = base.VisitParameter(parameter);

            TypeNode typeNode = (TypeNode)parameter.TypeExpression;
            parameterType.Add(typeNode);

            return re;
        }


        //get signature type, prepare for signature matching 
        private TypeNode signatureType;
        public TypeNode getSty()
        {
            return signatureType;
        }
        

        public override Method VisitMethod(Method method)
        {
            Method re = base.VisitMethod(method);
            
            System.Compiler.TypeNode sigTmp = re.ReturnType;
            if(sigTmp.TypeCode!=TypeCode.Empty)
                signatureType = sigTmp;


            return re;
        }
    }

}
