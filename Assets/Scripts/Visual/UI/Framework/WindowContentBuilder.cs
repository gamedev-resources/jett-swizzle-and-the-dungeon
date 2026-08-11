using UnityEngine;

namespace Dungeon.Visual.UI.Framework
{
    public abstract class WindowContentBuilder : MonoBehaviour
    {
        public abstract void Build(GameWindow window);
    }
}
