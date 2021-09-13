using System;

namespace Cradle
{
	public class TagSwitch: StoryOutput
    {
        public bool isAndrea;
        public TagSwitch(bool andrea)
        {
            this.isAndrea = andrea;
        }

		public override string ToString()
		{		
			return string.Format("{0} (tagswitch)", isAndrea);
		}
	}
}

