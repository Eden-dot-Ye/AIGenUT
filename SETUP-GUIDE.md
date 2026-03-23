# 🧪 AIGenUT 完整搭建指南

> 本指南将手把手教你搭建一个完整的 **AI 辅助单元测试自动生成** 演示项目。
> 包含两个核心能力：
> 1. **Copilot Skill** — 在开发时主动让 AI 生成测试
> 2. **GitHub Actions** — 在 PR 中自动检查测试覆盖率

---

## 目录

1. [前置准备](#1-前置准备)
2. [创建 GitHub 仓库](#2-创建-github-仓库)
3. [上传项目文件](#3-上传项目文件)
4. [验证项目能编译和运行测试](#4-验证项目能编译和运行测试)
5. [理解 Copilot Skill 的工作原理](#5-理解-copilot-skill-的工作原理)
6. [测试 Copilot Skill（生成单元测试）](#6-测试-copilot-skill生成单元测试)
7. [理解 GitHub Actions 的工作原理](#7-理解-github-actions-的工作原理)
8. [测试 GitHub Actions（PR 覆盖率检查）](#8-测试-github-actions-pr-覆盖率检查)
9. [完整演示流程](#9-完整演示流程)
10. [常见问题解答](#10-常见问题解答)

---

## 1. 前置准备

### 你需要安装的软件

| 软件 | 用途 | 安装方式 |
|------|------|---------|
| **Git** | 版本控制 | [git-scm.com](https://git-scm.com/) |
| **.NET 8 SDK** | 编译和运行 C# 项目 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **VS Code** | 代码编辑器 | [code.visualstudio.com](https://code.visualstudio.com/) |
| **GitHub Copilot** | AI 编程助手（需要订阅） | VS Code 扩展商店搜索 "GitHub Copilot" |
| **GitHub Copilot Chat** | AI 对话式编程 | VS Code 扩展商店搜索 "GitHub Copilot Chat" |

### 验证安装

打开终端（Terminal），运行以下命令确认安装成功：

```bash
git --version        # 应该显示 git version 2.x.x
dotnet --version     # 应该显示 8.0.x
```

---

## 2. 创建 GitHub 仓库

### 步骤 2.1：在 GitHub 上创建新仓库

1. 打开浏览器，登录 [github.com](https://github.com)
2. 点击右上角的 **"+"** → **"New repository"**
3. 填写信息：
   - **Repository name**: `AIGenUT`
   - **Description**: `AI-Assisted Unit Test Generation Demo`
   - **Visibility**: Public（公开，方便演示）
   - ✅ 勾选 **"Add a README file"**
4. 点击 **"Create repository"**

### 步骤 2.2：克隆到本地

```bash
# 替换 YOUR_USERNAME 为你的 GitHub 用户名
git clone https://github.com/YOUR_USERNAME/AIGenUT.git
cd AIGenUT
```

---

## 3. 上传项目文件

### 步骤 3.1：复制项目文件

将 `AIGenUT-Demo/` 目录中的**所有内容**复制到你克隆的 `AIGenUT/` 目录中。

目录结构应该是：
```
AIGenUT/                    ← 你克隆的仓库根目录
├── .github/
│   ├── skills/
│   │   └── cw-test-generator/
│   │       ├── SKILL.md
│   │       └── references/
│   │           ├── test-patterns.md
│   │           └── assertion-reference.md
│   └── workflows/
│       └── ai-test-review.yml
├── src/
│   ├── DemoLib/
│   │   ├── DemoLib.csproj
│   │   ├── StringHelper.cs
│   │   ├── ShipmentValidator.cs
│   │   └── WeightConverter.cs
│   └── DemoLib.Tests/
│       ├── DemoLib.Tests.csproj
│       └── StringHelperTest.cs
├── AIGenUT.sln
├── README.md
└── SETUP-GUIDE.md          ← 本文件
```

### 步骤 3.2：提交并推送到 GitHub

```bash
git add -A
git commit -m "Initial project setup with Copilot Skill and GitHub Actions"
git push origin main
```

---

## 4. 验证项目能编译和运行测试

在 `AIGenUT/` 目录下运行：

```bash
# 编译
dotnet build

# 运行测试
dotnet test
```

你应该看到类似这样的输出：
```
Passed!  - Failed:  0, Passed:  11, Skipped:  0, Total:  11
```

✅ 如果测试全部通过，说明项目搭建正确。

---

## 5. 理解 Copilot Skill 的工作原理

### 什么是 Copilot Skill？

简单来说，Copilot Skill 就是一份**"指导手册"**，放在仓库的 `.github/skills/` 目录下。
当你在 Copilot Chat 中提出请求时，Copilot 会**自动读取这份手册**，按照里面的规则来生成代码。

### Skill 文件在哪里？

```
.github/skills/cw-test-generator/
├── SKILL.md                          ← 主文件：告诉 AI 如何生成测试
└── references/
    ├── test-patterns.md              ← 参考资料：常见测试模式
    └── assertion-reference.md        ← 参考资料：断言语法速查
```

### SKILL.md 里写了什么？

打开 `.github/skills/cw-test-generator/SKILL.md` 看看，它包含：

1. **触发条件** — 什么时候这个 Skill 会被激活
2. **分析步骤** — AI 应该如何分析源代码
3. **生成规则** — 测试方法的命名规范、断言风格、文件结构
4. **示例** — 给 AI 看的"标准答案"（Few-Shot Learning）

### 为什么需要 Skill？

不用 Skill：AI 生成的测试可能不符合你的项目规范（命名方式不对、用了不该用的断言等）。
用了 Skill：AI 会**严格按照你定义的规范**来生成测试。

---

## 6. 测试 Copilot Skill（生成单元测试）

### 演示场景：为 WeightConverter.cs 生成测试

`WeightConverter.cs` 有 5 个公共方法，但目前**没有测试文件**。我们让 AI 来生成。

### 步骤 6.1：在 VS Code 中打开项目

```bash
code AIGenUT/
```

### 步骤 6.2：打开 Copilot Chat

在 VS Code 中：
- 按 `Ctrl+Shift+I`（Windows/Linux）或 `Cmd+Shift+I`（Mac）打开 Copilot Chat
- 或者点击左侧边栏的 Copilot 图标

### 步骤 6.3：让 Copilot 生成测试

在 Copilot Chat 中输入以下内容：

```
请按照 cw-test-generator skill 的规范，为 src/DemoLib/WeightConverter.cs 生成完整的单元测试。

要求：
1. 使用 NUnit 4 constraint-based 断言
2. 每个公共方法至少 3 个测试（正常、边界、异常）
3. 测试方法命名：Test[方法名]_When[条件]_Then[结果]
4. 输出完整的 .cs 文件内容
```

### 步骤 6.4：查看 AI 生成的测试

Copilot 应该生成类似这样的测试代码：

```csharp
using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class WeightConverterTest
{
    [Test]
    public void TestKgToLbs_WhenZero_ThenReturnsZero()
    {
        var result = WeightConverter.KgToLbs(0m);
        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void TestKgToLbs_WhenOneKg_ThenReturnsCorrectLbs()
    {
        var result = WeightConverter.KgToLbs(1m);
        Assert.That(result, Is.EqualTo(2.2046m));
    }

    [Test]
    public void TestKgToLbs_WhenNegative_ThenThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WeightConverter.KgToLbs(-1m));
    }

    // ... 更多测试 ...
}
```

### 步骤 6.5：保存并运行测试

1. 将生成的代码保存为 `src/DemoLib.Tests/WeightConverterTest.cs`
2. 运行测试：

```bash
dotnet test
```

### 步骤 6.6：同样为 ShipmentValidator 生成测试

```
请按照 cw-test-generator skill 的规范，为 src/DemoLib/ShipmentValidator.cs 生成完整的单元测试。
```

保存为 `src/DemoLib.Tests/ShipmentValidatorTest.cs`，然后再次运行 `dotnet test`。

---

## 7. 理解 GitHub Actions 的工作原理

### 什么是 GitHub Actions？

GitHub Actions 是 GitHub 提供的**自动化服务**。你可以设置规则，让它在特定事件发生时自动执行任务。

在我们的项目中：
- **事件**：有人提交了 Pull Request（PR）
- **任务**：自动检查 PR 中变更的源文件是否有对应的测试文件

### Workflow 文件在哪里？

```
.github/workflows/ai-test-review.yml    ← GitHub Actions 会自动识别这个文件
```

### 这个 Workflow 做了什么？

```
PR 提交后自动触发
        │
        ▼
    ┌───────────┐
    │ 1. 编译项目 │
    └─────┬─────┘
          │
          ▼
    ┌───────────────┐
    │ 2. 运行现有测试 │
    └───────┬───────┘
            │
            ▼
    ┌──────────────────┐
    │ 3. 检测变更了哪些  │
    │    源文件          │
    └────────┬─────────┘
             │
             ▼
    ┌──────────────────────┐
    │ 4. 检查每个源文件是否  │
    │    有对应的测试文件    │
    └──────────┬───────────┘
               │
               ▼
    ┌──────────────────────┐
    │ 5. 在 PR 上发表评论：  │
    │    📊 测试覆盖率报告  │
    └──────────────────────┘
```

---

## 8. 测试 GitHub Actions（PR 覆盖率检查）

### 演示场景：创建一个包含新源文件但没有测试的 PR

### 步骤 8.1：创建新分支

```bash
git checkout -b feature/add-date-helper
```

### 步骤 8.2：添加一个没有测试的新源文件

创建文件 `src/DemoLib/DateHelper.cs`：

```csharp
namespace DemoLib;

/// <summary>
/// Date formatting utilities for shipment documents.
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// Formats a date as ISO 8601 (yyyy-MM-dd).
    /// </summary>
    public static string ToIsoDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Returns the number of business days between two dates (excluding weekends).
    /// </summary>
    public static int BusinessDaysBetween(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date must be after start date.");

        int businessDays = 0;
        var current = start;
        while (current < end)
        {
            current = current.AddDays(1);
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;
        }
        return businessDays;
    }
}
```

### 步骤 8.3：提交并推送

```bash
git add -A
git commit -m "Add DateHelper utility"
git push origin feature/add-date-helper
```

### 步骤 8.4：创建 Pull Request

1. 打开 GitHub 网站，进入你的 `AIGenUT` 仓库
2. 你应该看到一个黄色提示条 "Compare & pull request"，点击它
3. 填写 PR 标题（例如 "Add DateHelper utility"）
4. 点击 **"Create pull request"**

### 步骤 8.5：等待 GitHub Actions 运行

1. 在 PR 页面，你会看到一个 "Checks" 区域
2. "AI Test Coverage Review" 会自动开始运行
3. 等待 1-2 分钟

### 步骤 8.6：查看自动生成的评论

GitHub Actions 会在 PR 下方自动发表一条评论，内容类似：

```
## 🧪 AI 测试覆盖率检查报告

### 📊 总览
| 指标 | 值 |
|------|------|
| 变更的源文件数 | 1 |
| 已有测试的文件 | 0 |
| 缺少测试的文件 | 1 |
| 测试覆盖率 | 0% |

### ⚠️ 状态: 有 1 个源文件缺少对应的单元测试。

### 📋 详细检查结果
- ❌ `src/DemoLib/DateHelper.cs` → **缺少**测试文件 `DateHelperTest.cs`

### 💡 建议
对于缺少测试的文件，你可以在 Copilot Chat 中使用 cw-test-generator skill 自动生成测试。
```

### 步骤 8.7：用 Copilot 生成测试并更新 PR

1. 在 VS Code 中用 Copilot 为 `DateHelper.cs` 生成测试
2. 保存为 `src/DemoLib.Tests/DateHelperTest.cs`
3. 提交并推送：

```bash
git add -A
git commit -m "Add unit tests for DateHelper"
git push origin feature/add-date-helper
```

4. GitHub Actions 会再次运行，这次评论会更新为：
```
### ✅ 状态: 所有变更的源文件都有对应的测试！
| 测试覆盖率 | 100% |
```

---

## 9. 完整演示流程

以下是比赛/展示时的推荐演示顺序（约 5-7 分钟）：

### 第一幕：展示问题（1 分钟）
- 打开 `WeightConverter.cs` —— 有 5 个方法，0 个测试
- "手动写测试很耗时，而且容易写错格式"

### 第二幕：Copilot Skill 生成测试（2 分钟）
- 打开 Copilot Chat
- 输入请求，展示 AI 如何在 10 秒内生成完整测试
- 运行 `dotnet test`，展示全部通过

### 第三幕：GitHub Actions 自动检查（2 分钟）
- 创建新分支，添加 `DateHelper.cs`（无测试）
- 创建 PR，展示 Actions 自动检测到缺少测试
- 用 Copilot 生成测试，推送更新
- 展示评论自动更新为 100%

### 第四幕：总结（1 分钟）
- "两层防护：开发时主动 + PR 时被动"
- "生成时间：30-60 分钟 → 10 秒"
- "规范符合率：~95%"

---

## 10. 常见问题解答

### Q: Copilot 没有按照 Skill 的规范生成？

**A:** 确保：
1. Skill 文件在 `.github/skills/cw-test-generator/SKILL.md` 路径下
2. 在 Copilot Chat 中明确提到 skill 名称
3. 用 `@workspace` 引用整个工作区，让 Copilot 能读取 Skill 文件

### Q: GitHub Actions 没有触发？

**A:** 检查：
1. workflow 文件在 `.github/workflows/ai-test-review.yml` 路径下
2. PR 的目标分支是 `main`
3. PR 中修改了 `src/DemoLib/` 目录下的 `.cs` 文件
4. 在仓库 Settings → Actions → General 中确认 Actions 已启用

### Q: 测试运行失败了？

**A:** 运行：
```bash
dotnet test --verbosity detailed
```
查看详细的错误信息。常见问题：
- 缺少 NuGet 包：运行 `dotnet restore`
- .NET SDK 版本不对：确认安装了 .NET 8

### Q: 如何在真实的 CargoWise 项目中使用？

**A:** 主要修改：
1. Skill 文件中的断言规范改为 CargoWise 自定义断言
2. 测试基类选择规则根据 `cw-testing` skill 调整
3. GitHub Actions 中的路径过滤根据实际项目结构调整

### Q: 我需要 GitHub Copilot 付费订阅吗？

**A:** 是的。GitHub Copilot 和 Copilot Chat 需要有效的订阅才能使用。
如果你的公司有 GitHub Copilot Business/Enterprise 许可证，可以直接使用。
个人订阅价格约为 $10/月。
