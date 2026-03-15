# 擦窗喷涂机器人数字孪生方案（Unity + C# 主控，Python 可选扩展）

## 1. 结论先说
- **Unity 侧行为控制建议必须用 C#**（Unity 运行时原生脚本语言）。
- **Python 非常适合做算法服务、数据中台、Web 观察面板后端**，与 Unity 通过 HTTP/WebSocket/MQTT 通信。
- 最稳妥架构：
  1. Unity(C#) 负责 3D 模型运动、擦拭/喷涂执行与可视化。
  2. Python(FastAPI) 负责状态聚合、策略计算、历史回放。
  3. Web 前端负责实时监控面板与任务下发。

## 2. 数字孪生目标拆解
1. **运动孪生**：机器人在玻璃平面路径运动（速度、加速度、姿态、碰边策略）。
2. **作业孪生**：擦拭覆盖率、喷涂覆盖率、重复覆盖、遗漏区域。
3. **状态孪生**：电量、液位、刷头压力、泵流量、告警。
4. **控制孪生**：任务参数（步进间距、喷嘴开关阈值）在线调整并实时反馈。
5. **数据孪生**：实时流 + 历史轨迹 + KPI（效率、能耗、覆盖率）。

## 3. 坐标与面积计算
- 约定玻璃为局部平面 `(u, v)`，范围 `[0,1] x [0,1]`。
- 将玻璃离散为 `rows x cols` 网格。
- 每帧根据机器人在玻璃平面的投影位置更新网格单元：
  - `wipeCoverage[cell] = true` 当擦拭半径覆盖到该格。
  - `paintCoverage[cell] = true` 当喷涂半径覆盖到该格且喷嘴开启。
- 面积计算：
  - 单格面积 `Acell = WindowWidth * WindowHeight / (rows*cols)`
  - 擦拭面积 `Awipe = count(wipeCoverage=true) * Acell`
  - 喷涂面积 `Apaint = count(paintCoverage=true) * Acell`

## 4. 实时通信建议
- Unity -> Python：`POST /telemetry`（10~20Hz）
- Python -> Web：`WebSocket /ws`（推送最新状态）
- Web -> Python -> Unity：任务策略下发（可扩展 `/command`）

## 5. 可扩展算法
- 路径规划：Z字扫描、螺旋扫描、边界优先补漏。
- 优化目标：最大覆盖率、最短时间、最低重叠率。
- 可做闭环控制：根据“覆盖热力图”动态补喷/补擦。
