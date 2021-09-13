using System;

namespace Cradle
{
	public class AudioDelay: StoryOutput
    {
        public float delayTime;
        public AudioDelay(float seconds)
        {
            this.delayTime = seconds;
        }

		public override string ToString()
		{		
			return string.Format("{0} (audiodelay)", delayTime);
		}
	}
}

