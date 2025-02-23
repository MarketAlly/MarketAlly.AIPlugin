using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class AIPluginAttribute : Attribute
	{
		public string Name { get; }
		public string Description { get; }

		public AIPluginAttribute(string name, string description)
		{
			Name = name;
			Description = description;
		}
	}
}
