using Microsoft.Xna.Framework;

namespace Penguinner
{
    public class Collison
    {
        public Collison()
        {

        }

        public bool IsCollided(Rectangle r1, Rectangle r2)
        {
            return r1.Intersects(r2);
        }
    }
}
