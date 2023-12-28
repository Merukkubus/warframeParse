

using warframeParse.Models.Entities;

namespace warframeParse.Models.ViewModels
{
    public class WarframeVM
    {
        public string Name { get; set; }

        public string Sex { get; set; }

        public int Health { get; set; }

        public int Shields { get; set; }

        public int Armor { get; set; }

        public int Energy { get; set; }

        public double Sprint_speed { get; set; }
    
        public string Element {  get; set; }

        public warframe_polarity Madurai { get; set; }

        public warframe_polarity Naramon { get; set; }

        public warframe_polarity Vazarin { get; set; }
    }
}
