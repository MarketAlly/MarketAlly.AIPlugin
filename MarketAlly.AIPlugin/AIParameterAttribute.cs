using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin
{
	[AttributeUsage(AttributeTargets.Property)]
	public class AIParameterAttribute : Attribute
	{
		public string Description { get; }
		public bool Required { get; }

		public AIParameterAttribute(string description, bool required = false)
		{
			Description = description;
			Required = required;
		}
	}
}
