using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class UserSaveRecipe
    {

        public int Id { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }

        public User User { get; set; }
        public Recipe Recipe { get; set; }
    }
}