using System;

namespace csharp_sccd_compiler
{
	abstract public class Visitor
	{
		abstract public class Visitable
		{
			public virtual void accept(Visitor visitor)
			{
				visitor.visit (this);
			}
		}

		public Visitor ()
		{
		}

		public void visit(Visitable visiting)
		{
		}
	}
}

