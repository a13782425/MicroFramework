using System.Collections.Generic;

namespace MFramework.Core
{
    /// <summary>
    /// A*寻路的图
    /// </summary>
    public interface IAStarGraph
    {
        /// <summary>
		/// 获取邻居节点
		/// </summary>
		IEnumerable<AStarNode> Neighbors(AStarNode node);

        /// <summary>
        /// 计算移动代价
        /// </summary>
        float CalculateCost(AStarNode from, AStarNode to);

        /// <summary>
        /// 清空所有节点的临时数据
        /// </summary>
        void ClearTemper();
    }
}
