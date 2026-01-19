#!/bin/bash
# Arcadia 素材生成辅助脚本
# 帮助快速调整 AI 生成的图片到正确尺寸

set -e

ASSETS_DIR=".tmp/arcadia-assets-generated"
PROMPTS_DIR=".tmp/arcadia-assets/prompts"

echo "🎨 Arcadia 像素风素材生成指南"
echo "================================"
echo ""
echo "由于 RunningHub API 限制，请使用以下方法生成素材："
echo ""

echo "方法 1：RunningHub 网页端（推荐）"
echo "-----------------------------------"
echo "1. 访问: https://www.runninghub.cn/ai-detail/1957729299266727938"
echo "2. 登录账号（微信扫码）"
echo "3. 复制提示词（见 prompts/ 目录）"
echo "4. 生成图片并下载"
echo "5. 运行此脚本调整尺寸"
echo ""

echo "方法 2：使用其他 AI 工具"
echo "---------------------------"
echo "- Midjourney: 添加 --pixelart --style raw 参数"
echo "- Stable Diffusion: 使用 pixel art LoRA"
echo "- DALL-E 3: 提示词添加 'pixel art style'"
echo ""

echo "方法 3：手工绘制（专业）"
echo "-----------------------"
echo "- Aseprite: https://www.aseprite.org（$19.99）"
echo "- LibreSprite: 免费（https://libresprite.github.io）"
echo "- Piskel: 在线免费（https://www.piskelapp.com）"
echo ""

echo "================================"
echo "生成后的处理步骤："
echo "================================"
echo ""

# 创建输出目录
mkdir -p "$ASSETS_DIR"

echo "1. 下载生成的图片到: $ASSETS_DIR"
echo "   保持原文件名，例如: char_adventurer_idle_001.png"
echo ""

echo "2. 运行尺寸调整脚本："
echo "   bash scripts/process_ai_assets.sh"
echo ""

echo "3. 验证命名和尺寸："
echo "   python3 scripts/check_asset_specs.py"
echo ""

echo "4. 移动到资源目录："
echo "   cp -r $ASSETS_DIR/* assets/"
echo ""

echo "================================"
echo "提示词示例（主角 idle）:"
echo "================================"
echo ""

cat <<'EOF'
pixel art game character, young adventurer warrior,
blue light armor with gold trim, idle standing pose,
side view, 48x48 pixels, clean sharp pixels,
deep dark fantasy style, transparent background,
high quality, masterpiece, RPG character sprite
EOF

echo ""
echo "================================"
echo "完整提示词见: $PROMPTS_DIR/character_prompts.md"
echo "================================"
