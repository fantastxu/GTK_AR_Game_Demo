# Cyberpunk 星星特效改造指南

## 📦 已创建的资源

### 1. 纹理生成器 (Editor Tool)
**路径**: `Assets/TheWorldBeyond/Environment/RoomEnvironment/Editor/CyberpunkTextureGenerator.cs`

**功能**: 程序化生成Cyberpunk风格纹理
- 电路板纹理 (Circuit Board)
- 数据流纹理 (Data Stream)
- 故障效果纹理 (Glitch Pattern)

### 2. Cyberpunk Shader
**路径**: `Assets/TheWorldBeyond/Environment/Shaders/OppyDimensionCyberpunk.shader`

**新增特效**:
- ⚡ 水平扫描线效果
- 🌈 霓虹青色/品红色配色
- 💥 故障艺术 (Glitch Art) 效果
- 📡 RGB色差分离
- 🔲 数字脉冲替代圆形涌动

---

## 🚀 使用步骤

### 步骤 1: 生成Cyberpunk纹理

1. 在Unity菜单栏选择: **TheWorldBeyond → Generate Cyberpunk Texture**

2. 在弹出的窗口中调整参数:
   - **Texture Size**: 512 (推荐) 或更高
   - **Circuit Density**: 0.3-0.5 (电路密度)
   - **Scanline Frequency**: 10-20 (扫描线频率)
   - **Primary Color**: 青色 (0, 1, 1) - Cyan
   - **Secondary Color**: 品红色 (1, 0, 1) - Magenta

3. 点击以下按钮之一生成纹理:
   - **Generate Circuit Board Texture** - 推荐用于墙面
   - **Generate Data Stream Texture** - 推荐用于动态效果
   - **Generate Glitch Pattern Texture** - 推荐用于家具

4. 纹理将保存到:
   - `Assets/TheWorldBeyond/Environment/RoomEnvironment/textures/CyberpunkPattern_Circuit.png`
   - `Assets/TheWorldBeyond/Environment/RoomEnvironment/textures/CyberpunkPattern_DataStream.png`
   - `Assets/TheWorldBeyond/Environment/RoomEnvironment/textures/CyberpunkPattern_Glitch.png`

---

### 步骤 2: 创建Cyberpunk材质

#### 方案 A: 复制现有材质并修改

1. 找到材质: `Assets/TheWorldBeyond/Environment/RoomEnvironment/materials/OppyDimensionMaterial.mat`

2. 复制材质并重命名为: `OppyDimensionMaterialCyberpunk.mat`

3. 在Inspector中:
   - 将 **Shader** 改为: `TheWorldBeyond/OppyDimensionCyberpunk`
   - 将 **MainTex** 设置为刚生成的纹理 (推荐: `CyberpunkPattern_Circuit.png`)
   - 调整以下参数:

   **Cyberpunk特效参数**:
   ```
   Neon Color: (0, 1, 1, 1)           # 青色
   Secondary Neon Color: (1, 0, 1, 1) # 品红色
   Scanline Speed: 2                   # 扫描线速度
   Scanline Frequency: 10-15           # 扫描线频率
   Glitch Intensity: 0.2-0.4           # 故障强度
   RGB Split Amount: 0.005-0.01        # RGB分离强度
   ```

   **原有参数保持**:
   ```
   Saturation Amount: 1.0
   Fog Strength: 根据场景调整
   Triplanar Falloff: 1.0
   Color: (0.2, 0.2, 0.3, 1)          # 可调整基础色调
   ```

#### 方案 B: 在Scene中直接替换材质

如果你想立即看到效果，可以通过代码在Runtime替换。

---

### 步骤 3: 应用到场景

#### 方法 1: 在Unity Editor中手动替换

1. 在场景中找到使用 `OppyDimensionMaterial` 的对象
2. 将材质替换为 `OppyDimensionMaterialCyberpunk`

#### 方法 2: 通过代码替换 (推荐)

修改 `WorldBeyondRoomObject.cs` 来使用新材质:

在 `Start()` 方法中添加:
```csharp
// 在 Line 35 附近
m_defaultMaterial = PassthroughMesh.material;

// 添加这行来替换为Cyberpunk材质
Material cyberpunkMat = Resources.Load<Material>("OppyDimensionMaterialCyberpunk");
if (cyberpunkMat != null)
{
    PassthroughMesh.material = cyberpunkMat;
    m_defaultMaterial = cyberpunkMat;
}
```

或者创建一个公共变量让你在Inspector中指定:
```csharp
public Material CyberpunkMaterial; // 在Inspector中拖拽赋值

private void Start()
{
    if (CyberpunkMaterial != null)
    {
        PassthroughMesh.material = CyberpunkMaterial;
        m_defaultMaterial = CyberpunkMaterial;
    }
    else
    {
        m_defaultMaterial = PassthroughMesh.material;
    }
}
```

---

## 🎨 效果调优建议

### 如果效果太强烈:
- 降低 `Glitch Intensity` 到 0.1-0.2
- 降低 `RGB Split Amount` 到 0.002
- 降低 `Scanline Frequency` 到 5-8

### 如果效果太弱:
- 提高 `Neon Color` 的亮度 (例如: (0, 1.5, 1.5, 1))
- 提高 `Scanline Speed` 到 3-5
- 使用 `CyberpunkPattern_DataStream.png` 纹理（更动态）

### 更符合Cyberpunk风格:
- **主色调**: 深蓝紫色 `Color: (0.1, 0.1, 0.3, 1)`
- **霓虹色**: 保持青色和品红色
- **增加对比度**: 调高 `Saturation Amount` 到 1.2-1.5

---

## 🔧 进阶自定义

### 修改扫描方向

在 `OppyDimensionCyberpunk.shader` 中修改第 201-204 行:

```hlsl
// 垂直扫描 (从下往上)
half scanline = sin((i.worldPos.y + _Time.y * _ScanlineSpeed) * _ScanlineFrequency);

// 改为水平扫描 (从左往右)
half scanline = sin((i.worldPos.x + _Time.y * _ScanlineSpeed) * _ScanlineFrequency);

// 或者对角线扫描
half scanline = sin(((i.worldPos.x + i.worldPos.y) + _Time.y * _ScanlineSpeed) * _ScanlineFrequency);
```

### 修改故障效果频率

在第 173-180 行修改:
```hlsl
half glitchTime = floor(_Time.y * 10.0); // 改为 5.0 更慢, 20.0 更快
```

### 添加数字雨效果

可以在fragment shader中添加:
```hlsl
// 数字雨效果
half digitalRain = frac((i.worldPos.y * 10) - (_Time.y * 2));
digitalRain = step(0.95, digitalRain); // 只保留最亮的部分
finalColor += _NeonColor.rgb * digitalRain * 0.3;
```

---

## 📝 注意事项

1. **性能优化**:
   - 生成的纹理建议不超过 1024x1024
   - 如果卡顿，降低 `Glitch Intensity`

2. **与原版对比**:
   - 新shader保持了所有原版参数
   - 可以随时通过切换shader恢复原版效果

3. **材质实例化**:
   - 如果在运行时修改参数，建议使用 `materialPropertyBlock` 避免材质实例化

4. **测试建议**:
   - 先在一个墙面上测试效果
   - 满意后再应用到所有墙面和家具

---

## 🎯 快速测试

最快的测试方法:

1. 运行纹理生成器，生成 `Circuit Board` 纹理
2. 创建新材质，使用 `OppyDimensionCyberpunk` shader
3. 在Scene中找到一个墙面对象，临时替换材质
4. 进入Play模式查看效果
5. 在Inspector中实时调整参数看效果

---

## 🐛 故障排除

### 问题: 看不到任何效果
- 检查材质是否正确分配shader
- 检查 `_MainTex` 是否分配了纹理
- 检查 `_OppyRippleStrength` 是否大于 0

### 问题: 颜色太暗
- 提高 `Neon Color` 和 `Secondary Neon Color` 的亮度值
- 降低 `Fog Strength`
- 提高 `Saturation Amount`

### 问题: 故障效果太多
- 降低 `Glitch Intensity` 到 0.1 以下
- 降低 `RGB Split Amount`

---

好运！享受Cyberpunk风格的效果！🌃
