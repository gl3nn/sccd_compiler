using System;
using csharp_sccd_compiler;
using NUnit.Framework;

namespace csharp_tests
{
    [TestFixture]
    [Category("CSharp")]
    public class CSharpTests : TestsBase
    {
        [TestFixtureSetUp]
        public void setLogger()
        {
            Logger.verbose = 1;
        }

        protected override bool generate(string file_path, string expected_exception)
        {
            TestDelegate generation_delegate = () => Compiler.generate(file_path, this.path_generated_code, CodeGenerator.Platform.THREADS);

            if (expected_exception == null)
                generation_delegate();
            else
            {
                Type expected_exception_type = null;
                if (expected_exception == "CompilerException")
                    expected_exception_type = typeof(CompilerException);
                else if (expected_exception == "TransitionException")
                    expected_exception_type = typeof(TransitionException);
                else
                    Assert.Fail("Invalid value for the exception attribute.");

                Exception throwed_exception = Assert.Catch<Exception>(generation_delegate, "Expected an exception but none was throwed.").GetBaseException();
                Assert.IsInstanceOf(expected_exception_type, throwed_exception, 
                                        string.Format("Expected exception of type {0} but got an exception of type {1} instead.", expected_exception_type, throwed_exception.GetType()));
                return false;
            }
            return true;
        }
    }
}

