using UnityEngine;

namespace SFG.AStar
{
    public class GridNodes
    {
        private int m_Width;
        private int m_Height;
        private Node[,] m_GridNodes;

        /// <summary>
        /// 构造函数初始化网格节点数组
        /// </summary>
        /// <param name="width">地图宽度</param>
        /// <param name="height">地图高度</param>
        public GridNodes(int width, int height)
        {
            m_Width = width;
            m_Height = height;

            m_GridNodes = new Node[m_Width, m_Height];
            for (int x = 0; x < m_Width; ++x)
            {
                for (int y = 0; y < m_Height; ++y)
                {
                    m_GridNodes[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// 获取网格节点
        /// </summary>
        /// <param name="x">x轴坐标</param>
        /// <param name="y">y轴坐标</param>
        /// <returns>如果存在，根据传入的 x，y 返回一个网格节点</returns>
        public Node GetGridNode(int x, int y)
        {
            if (x < m_Width && y < m_Height)
            {
                return m_GridNodes[x, y];
            }

            Debug.Log("超出网格范围");
            return null;
        }
    }
}