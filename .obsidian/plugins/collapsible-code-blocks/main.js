/*
THIS IS A GENERATED/BUNDLED FILE BY ESBUILD
if you want to view the source, please visit the github repository of this plugin
*/

var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

// src/main.ts
var main_exports = {};
__export(main_exports, {
  default: () => CollapsibleCodeBlockPlugin
});
module.exports = __toCommonJS(main_exports);
var import_obsidian3 = require("obsidian");

// src/types.ts
var DEFAULT_SETTINGS = {
  defaultCollapsed: false,
  collapseIcon: "\u25BC",
  expandIcon: "\u25B6",
  enableHorizontalScroll: true,
  collapsedLines: 0
};

// src/editView.ts
var import_view = require("@codemirror/view");
var import_language = require("@codemirror/language");
var import_state = require("@codemirror/state");
var import_obsidian = require("obsidian");
var toggleFoldEffect = import_state.StateEffect.define();
var _FoldWidget = class extends import_view.WidgetType {
  constructor(startPos, endPos, settings, foldField, app) {
    super();
    this.startPos = startPos;
    this.endPos = endPos;
    this.settings = settings;
    this.foldField = foldField;
    this.app = app;
  }
  static getFrontmatterCodeBlockState(app) {
    var _a;
    const activeView = app.workspace.getActiveViewOfType(import_obsidian.MarkdownView);
    if (!(activeView == null ? void 0 : activeView.file))
      return null;
    const cache = app.metadataCache.getFileCache(activeView.file);
    if (!((_a = cache == null ? void 0 : cache.frontmatter) == null ? void 0 : _a["code-blocks"]))
      return null;
    const value = cache.frontmatter["code-blocks"].toLowerCase();
    return value === "collapsed" ? true : value === "expanded" ? false : null;
  }
  getBlockId() {
    var _a;
    const file = (_a = this.app.workspace.getActiveViewOfType(import_obsidian.MarkdownView)) == null ? void 0 : _a.file;
    return `${(file == null ? void 0 : file.path) || ""}-${this.startPos}-${this.endPos}`;
  }
  initializeCodeBlock(view) {
    const blockId = this.getBlockId();
    if (_FoldWidget.initializedBlocks.has(blockId)) {
      return;
    }
    _FoldWidget.initializedBlocks.add(blockId);
    const frontmatterState = _FoldWidget.getFrontmatterCodeBlockState(this.app);
    const shouldCollapse = frontmatterState !== null ? frontmatterState : this.settings.defaultCollapsed;
    if (shouldCollapse) {
      let isAlreadyFolded = false;
      view.state.field(this.foldField).between(this.startPos, this.endPos, () => {
        isAlreadyFolded = true;
      });
      if (!isAlreadyFolded) {
        requestAnimationFrame(() => {
          view.dispatch({
            effects: toggleFoldEffect.of({
              from: this.startPos,
              to: this.endPos,
              defaultState: true
            })
          });
        });
      }
    }
  }
  toDOM(view) {
    const button = document.createElement("div");
    button.className = "code-block-toggle";
    let isFolded = false;
    view.state.field(this.foldField).between(this.startPos, this.endPos, () => {
      isFolded = true;
    });
    this.initializeCodeBlock(view);
    button.innerHTML = isFolded ? this.settings.expandIcon : this.settings.collapseIcon;
    button.setAttribute("aria-label", isFolded ? "Expand code block" : "Collapse code block");
    button.onclick = (e) => {
      e.preventDefault();
      e.stopPropagation();
      view.dispatch({
        effects: toggleFoldEffect.of({
          from: this.startPos,
          to: this.endPos,
          defaultState: isFolded ? false : true
        })
      });
    };
    return button;
  }
  // Add method to clear initialization state when switching files
  static clearInitializedBlocks() {
    _FoldWidget.initializedBlocks.clear();
  }
};
var FoldWidget = _FoldWidget;
FoldWidget.initializedBlocks = /* @__PURE__ */ new Set();
var createFoldField = (settings) => import_state.StateField.define({
  create() {
    return import_view.Decoration.none;
  },
  update(folds, tr) {
    folds = folds.map(tr.changes);
    for (let effect of tr.effects) {
      if (effect.is(toggleFoldEffect)) {
        const { from, to, defaultState } = effect.value;
        let hasFold = false;
        folds.between(from, to, () => {
          hasFold = true;
        });
        if (defaultState === false) {
          folds = folds.update({
            filter: (fromPos, toPos) => fromPos !== from || toPos !== to
          });
        } else {
          const capturedFrom = from;
          const capturedTo = to;
          const deco = import_view.Decoration.replace({
            block: true,
            inclusive: true,
            widget: new class extends import_view.WidgetType {
              toDOM(view) {
                const container = document.createElement("div");
                container.className = "code-block-folded";
                container.style.setProperty("--collapsed-lines", settings.collapsedLines.toString());
                const contentDiv = document.createElement("div");
                contentDiv.className = "folded-content";
                const lines = view.state.doc.sliceString(capturedFrom, capturedTo).split("\n").slice(0, settings.collapsedLines).join("\n");
                contentDiv.textContent = lines;
                const button = document.createElement("div");
                button.className = "code-block-toggle";
                button.textContent = settings.expandIcon;
                button.onclick = (e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  view.dispatch({
                    effects: toggleFoldEffect.of({
                      from: capturedFrom,
                      to: capturedTo,
                      defaultState: false
                    })
                  });
                };
                container.appendChild(button);
                container.appendChild(contentDiv);
                return container;
              }
            }()
          });
          folds = folds.update({
            add: [deco.range(from, to)]
          });
        }
      }
    }
    return folds;
  },
  provide: (f) => import_view.EditorView.decorations.from(f)
});
var codeBlockPositions = import_state.StateField.define({
  create(state) {
    return findCodeBlockPositions(state);
  },
  update(value, tr) {
    return findCodeBlockPositions(tr.state);
  }
});
function findCodeBlockPositions(state) {
  const positions = [];
  (0, import_language.syntaxTree)(state).iterate({
    enter: (node) => {
      const nodeName = node.type.name;
      if (nodeName.includes("HyperMD-codeblock-begin")) {
        const lineEl = document.querySelector(`.${nodeName}`);
        if (lineEl && lineEl.parentElement) {
          lineEl.parentElement.classList.add("ccb-editor-codeblock");
        }
        const line = state.doc.lineAt(node.from);
        if (line.text.trim().startsWith("```")) {
          let endFound = false;
          for (let i = line.number; i <= state.doc.lines; i++) {
            const currentLine = state.doc.line(i);
            if (i !== line.number && currentLine.text.trim().startsWith("```")) {
              positions.push({
                startPos: line.from,
                endPos: currentLine.to
              });
              endFound = true;
              break;
            }
          }
          if (!endFound) {
            positions.push({
              startPos: line.from,
              endPos: state.doc.line(state.doc.lines).to
            });
          }
        }
      }
    }
  });
  return positions;
}
function buildDecorations(state, settings, foldField, app) {
  const widgets = [];
  const positions = state.field(codeBlockPositions);
  positions.forEach((pos) => {
    const widget = import_view.Decoration.widget({
      widget: new FoldWidget(pos.startPos, pos.endPos, settings, foldField, app),
      // Pass app as the fifth argument
      side: -1
    });
    widgets.push(widget.range(pos.startPos));
  });
  return import_view.Decoration.set(widgets, true);
}
function setupEditView(settings, app) {
  const foldField = createFoldField(settings);
  const currentDecorations = import_state.StateField.define({
    create(state) {
      return buildDecorations(state, settings, foldField, app);
    },
    update(value, transaction) {
      return buildDecorations(transaction.state, settings, foldField, app);
    },
    provide(field) {
      return import_view.EditorView.decorations.from(field);
    }
  });
  return [
    codeBlockPositions,
    foldField,
    currentDecorations
  ];
}

// src/readView.ts
var import_obsidian2 = require("obsidian");
function setupReadView(app, settings) {
  function getFrontmatterCodeBlockState() {
    var _a;
    const activeView = app.workspace.getActiveViewOfType(import_obsidian2.MarkdownView);
    if (!(activeView == null ? void 0 : activeView.file))
      return null;
    const cache = app.metadataCache.getFileCache(activeView.file);
    if (!((_a = cache == null ? void 0 : cache.frontmatter) == null ? void 0 : _a["code-blocks"]))
      return null;
    const value = cache.frontmatter["code-blocks"].toLowerCase();
    if (value === "collapsed")
      return true;
    if (value === "expanded")
      return false;
    return null;
  }
  function createToggleButton() {
    const button = document.createElement("div");
    button.className = "code-block-toggle";
    button.textContent = settings.collapseIcon;
    button.setAttribute("role", "button");
    button.setAttribute("tabindex", "0");
    button.setAttribute("aria-label", "Toggle code block visibility");
    const toggleHandler = (e) => {
      e.preventDefault();
      const pre = e.target.closest("pre");
      if (!pre)
        return;
      pre.classList.toggle("collapsed");
      updateCodeBlockVisibility(pre, true);
      const isCollapsed = pre.classList.contains("collapsed");
      button.textContent = isCollapsed ? settings.expandIcon : settings.collapseIcon;
      button.setAttribute("aria-expanded", (!isCollapsed).toString());
      app.workspace.requestSaveLayout();
    };
    button.addEventListener("click", toggleHandler);
    button.addEventListener("keydown", (e) => {
      if (e.key === "Enter" || e.key === " ") {
        e.preventDefault();
        toggleHandler(e);
      }
    });
    return button;
  }
  function updateCodeBlockVisibility(pre, forceRefresh = false) {
    var _a;
    const isCollapsed = pre.classList.contains("collapsed");
    const markdownView = app.workspace.getActiveViewOfType(import_obsidian2.MarkdownView);
    if (!((_a = markdownView == null ? void 0 : markdownView.previewMode) == null ? void 0 : _a.containerEl))
      return;
    const previewElement = markdownView.previewMode.containerEl;
    const rect = pre.getBoundingClientRect();
    const scrollTop = previewElement.scrollTop;
    const elementTop = rect.top + scrollTop;
    let curr = pre.nextElementSibling;
    while (curr && !(curr instanceof HTMLPreElement)) {
      if (curr instanceof HTMLElement) {
        if (isCollapsed) {
          curr.classList.add("element-hidden");
          curr.classList.remove("element-visible", "element-spacing");
        } else {
          curr.classList.remove("element-hidden");
          curr.classList.add("element-visible");
        }
      }
      curr = curr.nextElementSibling;
    }
    void pre.offsetHeight;
    const triggerReflow = async () => {
      window.dispatchEvent(new Event("resize"));
      await new Promise((resolve) => requestAnimationFrame(resolve));
      const originalScroll = previewElement.scrollTop;
      previewElement.scrollTop = Math.max(0, previewElement.scrollHeight - previewElement.clientHeight);
      await new Promise((resolve) => requestAnimationFrame(resolve));
      previewElement.scrollTop = originalScroll;
      window.dispatchEvent(new Event("resize"));
      await new Promise((resolve) => setTimeout(resolve, 50));
      window.dispatchEvent(new Event("resize"));
    };
    triggerReflow();
  }
  function setupCodeBlock(pre) {
    document.documentElement.style.setProperty("--collapsed-lines", settings.collapsedLines.toString());
    const toggleButton = createToggleButton();
    pre.insertBefore(toggleButton, pre.firstChild);
    const frontmatterState = getFrontmatterCodeBlockState();
    const shouldCollapse = frontmatterState !== null ? frontmatterState : settings.defaultCollapsed;
    if (shouldCollapse) {
      pre.classList.add("collapsed");
      toggleButton.textContent = settings.expandIcon;
      updateCodeBlockVisibility(pre, true);
    }
  }
  function processNewCodeBlocks(element) {
    element.querySelectorAll("pre:not(.has-collapse-button)").forEach((pre) => {
      if (!(pre instanceof HTMLElement))
        return;
      pre.classList.add("has-collapse-button", "ccb-code-block");
      const codeElement = pre.querySelector("code");
      if (codeElement) {
        codeElement.classList.add("ccb-hide-vertical-scrollbar");
      }
      setupCodeBlock(pre);
    });
  }
  function setupContentObserver(processNewCodeBlocks2) {
    return new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.type === "childList") {
          processNewCodeBlocks2(mutation.target);
        }
      });
    });
  }
  return {
    processNewCodeBlocks,
    setupContentObserver
  };
}

// src/main.ts
var CollapsibleCodeBlockPlugin = class extends import_obsidian3.Plugin {
  async onload() {
    await this.loadSettings();
    this.updateScrollSetting();
    document.documentElement.style.setProperty("--collapsed-lines", this.settings.collapsedLines.toString());
    const editorExtensions = setupEditView(this.settings, this.app);
    this.registerEditorExtension(editorExtensions);
    this.registerEvent(
      this.app.workspace.on("file-open", () => {
        FoldWidget.clearInitializedBlocks();
      })
    );
    this.readViewAPI = setupReadView(this.app, this.settings);
    this.contentObserver = this.readViewAPI.setupContentObserver(this.readViewAPI.processNewCodeBlocks);
    this.registerMarkdownPostProcessor((element) => {
      this.readViewAPI.processNewCodeBlocks(element);
      this.contentObserver.observe(element, { childList: true, subtree: true });
    });
    this.addSettingTab(new CollapsibleCodeBlockSettingTab(this.app, this));
  }
  updateScrollSetting() {
    document.body.setAttribute("data-ccb-horizontal-scroll", this.settings.enableHorizontalScroll.toString());
  }
  sanitizeIcon(icon) {
    const cleaned = icon.trim();
    if (cleaned.length <= 2) {
      return cleaned;
    } else {
      if (this.app.customIcons && this.app.customIcons.exists(cleaned)) {
        return cleaned;
      }
      return DEFAULT_SETTINGS.collapseIcon;
    }
  }
  async loadSettings() {
    var _a, _b;
    const loadedData = await this.loadData();
    this.settings = {
      ...DEFAULT_SETTINGS,
      ...loadedData,
      collapseIcon: this.sanitizeIcon((_a = loadedData == null ? void 0 : loadedData.collapseIcon) != null ? _a : DEFAULT_SETTINGS.collapseIcon),
      expandIcon: this.sanitizeIcon((_b = loadedData == null ? void 0 : loadedData.expandIcon) != null ? _b : DEFAULT_SETTINGS.expandIcon)
    };
  }
  async saveSettings() {
    this.settings.collapseIcon = this.sanitizeIcon(this.settings.collapseIcon);
    this.settings.expandIcon = this.sanitizeIcon(this.settings.expandIcon);
    await this.saveData(this.settings);
  }
  onunload() {
    var _a;
    (_a = this.contentObserver) == null ? void 0 : _a.disconnect();
    document.body.removeAttribute("data-ccb-horizontal-scroll");
  }
};
var CollapsibleCodeBlockSettingTab = class extends import_obsidian3.PluginSettingTab {
  constructor(app, plugin) {
    super(app, plugin);
    this.app = app;
    this.plugin = plugin;
  }
  display() {
    const { containerEl } = this;
    containerEl.empty();
    new import_obsidian3.Setting(containerEl).setName("Default collapsed state").setDesc("Should code blocks be collapsed by default?").addToggle((toggle) => toggle.setValue(this.plugin.settings.defaultCollapsed).onChange(async (value) => {
      this.plugin.settings.defaultCollapsed = value;
      await this.plugin.saveSettings();
    }));
    new import_obsidian3.Setting(containerEl).setName("Collapse icon").setDesc("Icon to show when code block is expanded (single character or emoji only)").addText((text) => text.setValue(this.plugin.settings.collapseIcon).onChange(async (value) => {
      const sanitized = value.trim();
      if (sanitized.length <= 2) {
        this.plugin.settings.collapseIcon = sanitized || DEFAULT_SETTINGS.collapseIcon;
        await this.plugin.saveSettings();
      }
    }));
    new import_obsidian3.Setting(containerEl).setName("Expand icon").setDesc("Icon to show when code block is collapsed (single character or emoji only)").addText((text) => text.setValue(this.plugin.settings.expandIcon).onChange(async (value) => {
      const sanitized = value.trim();
      if (sanitized.length <= 2) {
        this.plugin.settings.expandIcon = sanitized || DEFAULT_SETTINGS.expandIcon;
        await this.plugin.saveSettings();
      }
    }));
    new import_obsidian3.Setting(containerEl).setName("Enable horizontal scrolling").setDesc("Allow code blocks to scroll horizontally instead of wrapping text.").addToggle((toggle) => toggle.setValue(this.plugin.settings.enableHorizontalScroll).onChange(async (value) => {
      this.plugin.settings.enableHorizontalScroll = value;
      await this.plugin.saveSettings();
      this.plugin.updateScrollSetting();
    }));
    const collapsedLinesSetting = new import_obsidian3.Setting(containerEl).setName("Collapsed lines").setDesc("Number of lines visible when code block is collapsed");
    collapsedLinesSetting.addText((text) => {
      text.setValue(this.plugin.settings.collapsedLines.toString()).onChange(async (value) => {
        const numericValue = parseInt(value, 10);
        this.plugin.settings.collapsedLines = isNaN(numericValue) || numericValue < 0 ? 0 : numericValue;
        await this.plugin.saveSettings();
        document.documentElement.style.setProperty("--collapsed-lines", this.plugin.settings.collapsedLines.toString());
      });
    });
  }
};


/* nosourcemap */