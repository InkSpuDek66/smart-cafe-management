using System;
using System.Collections.Generic;

namespace Project_CSI402_T2_Y3.Models.Db;

public partial class Recipe
{
    public int RecipeId { get; set; }

    public int? MenuItemId { get; set; }

    public int? IngredientId { get; set; }

    public float? QuantityRequired { get; set; }

    public string? Unit { get; set; }
}
