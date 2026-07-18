namespace BookManagementApp.Models
{
    /// <summary>
    /// _CategoryPicker partial'ının modeli.
    /// ViewData yerine tipli model kullanılıyor: ViewData ile geçerken
    /// "Categories" anahtarı ViewBag.Categories ile çakışıp çalışma zamanında
    /// "An item with the same key has already been added" hatası veriyordu.
    /// </summary>
    public class CategoryPickerModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public int? SelectedId { get; set; }

        /// <summary>Form alanının name'i (model binding buna bakar).</summary>
        public string InputName { get; set; } = "CategoryId";

        /// <summary>Boş bırakılırsa InputName kullanılır.</summary>
        public string? InputId { get; set; }

        public bool Required { get; set; }

        public string ResolvedId => string.IsNullOrWhiteSpace(InputId) ? InputName : InputId;
    }
}
