using System;

namespace csharp_sccd_compiler
{
	abstract public class Visitor
	{
		public Visitor ()
		{
		}

		public virtual void visit(Visitable visiting)
		{
		}
	}
}

