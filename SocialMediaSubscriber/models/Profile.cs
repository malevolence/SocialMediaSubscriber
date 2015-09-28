using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaSubscriber.Models
{
	public class Profile
	{
		public int Id { get; set; }
		public string FullName { get; set; }
		public string Twitter { get; set; }
		public string Facebook { get; set; }
	}
}
