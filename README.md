# my-project
My first GitHub project 一个AI项目

## 现在什么情况（你可以直接看这里）
- 我把上一版补强成了**更可跑、可观测、可扩展**的一版：
  1. Unity 继续用 C# 做行为控制（运动/擦拭/喷涂/覆盖率）；
  2. Python 后端新增健康检查、命令入口、更稳的模板读取；
  3. 面板新增断线重连、连接状态、速度/面积/位置等关键监控；
  4. 提供 `requirements.txt` 和 `simulator.py`，即使没接 Unity 也能先演示数据流。

## 擦窗机器人数字孪生（Unity + C# + Python）示例

这个仓库提供了一个可落地骨架：
- Unity 侧：C# 控制三维模型运动、擦拭、喷涂，并计算覆盖率。
- Python 侧：FastAPI 接收遥测并通过 WebSocket 推送到可视化面板。
- Web 面板：实时显示覆盖率、电量、液位、位置和连接状态。

### 目录
- `unity/Scripts/`：Unity C# 脚本
- `python_backend/app.py`：后端服务
- `python_backend/templates/dashboard.html`：实时观察面板
- `python_backend/simulator.py`：本地模拟遥测生成器
- `python_backend/requirements.txt`：后端依赖
- `docs/digital-twin-architecture.md`：方案说明

## 关键回答

### 一定要用 C# 吗？
- **Unity 内部驱动模型行为（Transform、动画、粒子、材质）建议用 C#。**
- Python 适合做：路径规划算法、数据服务、历史分析、Web 控制台。

### 可以用 Python 做网站前端并纳入算法吗？
可以。典型方案是：
1. Unity(C#) 每 50~100ms 上传机器人状态到 Python；
2. Python 运行算法并维护数字孪生状态；
3. 前端通过 WebSocket 实时订阅状态，并可下发策略。

## 快速启动（Python 面板）

```bash
python -m pip install -r python_backend/requirements.txt
python -m uvicorn python_backend.app:app --reload --host 0.0.0.0 --port 8000
```

浏览器打开 `http://127.0.0.1:8000`。

### 没接 Unity 怎么演示？
```bash
python python_backend/simulator.py
```
这会持续向后端发送模拟遥测，面板能实时看到数据变化。

## Unity 集成步骤（简要）
1. 将 Blender 轻量化模型导入 Unity 场景。
2. 创建玻璃参考节点 `windowOrigin`（中心对齐玻璃平面）。
3. 给场景物体挂载：`CoverageGrid`、`WindowRobotController`、`TelemetryHttpPublisher`。
4. 在 `WindowRobotController` 中配置窗口尺寸、速度、喷涂与擦拭半径。
5. 将 `TelemetryHttpPublisher.telemetryEndpoint` 指向后端地址。
