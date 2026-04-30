import { EditorView, basicSetup } from "https://esm.sh/codemirror@6.0.1";
import { StreamLanguage } from "https://esm.sh/@codemirror/language@6.10.8";
import { HighlightStyle, syntaxHighlighting } from "https://esm.sh/@codemirror/language@6.10.8";
import { tags } from "https://esm.sh/@lezer/highlight@1.2.1";

const swmLanguage = StreamLanguage.define({
  token(stream) {
    if (stream.eatSpace()) return null;

    if (stream.match(/\[[^\]]+\]/)) return "variableName";
    if (stream.match(/https:\/\/swmaestro\.ai\/\S+/)) return "function link";
    if (stream.match(/https:\/\/notion\.so\/\S+/)) return "string link";
    if (stream.match(/-?\d+/)) return "number";

    if (stream.match(/안녕하세요|멘토입니다|멘토 소개:|이번에/)) return "keyword";
    if (stream.match(/신청 바랍니다|한자리 남았습니다|마감되었습니다\. 감사합니다!/)) return "keyword";
    if (stream.match(/아직 마감되지 않아 한번 더 공지드립니다/)) return "keyword control";
    if (stream.match(/인원 미달이라|인원이 미달이더라도|참고 부탁드립니다/)) return "keyword control";
    if (stream.match(/많은 관심 부탁드립니다|현재 인원 공유드립니다/)) return "atom";
    if (stream.match(/신청 링크:|\(정원|\)|잔여|명입니다|자리|남았습니다|명 부족합니다|을 개설했습니다/)) return "operator";

    stream.next();
    return null;
  }
});

const swmHighlight = HighlightStyle.define([
  { tag: tags.keyword, color: "#569cd6" },
  { tag: tags.controlKeyword, color: "#c586c0" },
  { tag: tags.variableName, color: "#9cdcfe" },
  { tag: tags.number, color: "#b5cea8" },
  { tag: tags.string, color: "#ce9178" },
  { tag: tags.link, color: "#4fc1ff", textDecoration: "underline" },
  { tag: tags.atom, color: "#4ec9b0" },
  { tag: tags.operator, color: "#d4d4d4" }
]);

function dispatchTextareaInput(textarea, value) {
  textarea.value = value;
  textarea.dispatchEvent(new Event("input", { bubbles: true }));
  textarea.dispatchEvent(new Event("change", { bubbles: true }));
}

function mountEditor(textarea) {
  if (textarea.dataset.swmEditor === "mounted") return;
  textarea.dataset.swmEditor = "mounted";

  const host = document.createElement("div");
  host.className = "swm-editor-host";
  textarea.after(host);
  textarea.classList.add("swm-editor-mounted");

  let syncingFromEditor = false;

  const view = new EditorView({
    doc: textarea.value,
    parent: host,
    extensions: [
      basicSetup,
      swmLanguage,
      syntaxHighlighting(swmHighlight),
      EditorView.updateListener.of((update) => {
        if (!update.docChanged) return;
        syncingFromEditor = true;
        dispatchTextareaInput(textarea, update.state.doc.toString());
        syncingFromEditor = false;
      }),
      EditorView.theme({
        "&": { height: "100%" },
        ".cm-scroller": { overflow: "auto" }
      })
    ]
  });

  window.setInterval(() => {
    if (syncingFromEditor) return;

    const editorValue = view.state.doc.toString();
    if (textarea.value !== editorValue) {
      view.dispatch({
        changes: { from: 0, to: editorValue.length, insert: textarea.value }
      });
    }
  }, 250);
}

function mountEditors() {
  document.querySelectorAll("textarea.swm-code-input").forEach(mountEditor);
}

mountEditors();
new MutationObserver(mountEditors).observe(document.body, { childList: true, subtree: true });
