using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;

namespace MatKollen.ViewModels
{
    public class RecipeDetailsViewModel
    {
        public required Recipe Recipe { get; set; }
        public required string Category { get; set; }
        public required string Username { get; set; }
    }
}