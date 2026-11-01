// 롤링페이퍼 API. 판(누구에게)은 lab_rolling_boards, 쪽지는 lab_rolling_msgs 표에 저장해요.
// 판마다 6글자 번호가 붙고, 그 번호가 들어간 링크(…/apps/rolling.html?b=번호)로 친구들이 같은 판에 모여요.
// GET ?b=번호: { board, msgs }   POST { action: "board", to }: 새 판 → { id }   POST { action: "msg", b, from, text, color }
const sb = (path, init = {}) =>
  fetch(`${process.env.SB_URL}/rest/v1/${path}`, {
    ...init,
    headers: { apikey: process.env.SB_ANON_KEY, Authorization: `Bearer ${process.env.SB_ANON_KEY}`, "Content-Type": "application/json", ...init.headers },
  });

const body = (req) => (typeof req.body === "string" ? JSON.parse(req.body || "{}") : req.body || {});
const clean = (s, n) => String(s || "").trim().slice(0, n);
const COLORS = ["yellow", "pink", "mint", "sky", "lilac"];
const newId = () => Array.from({ length: 6 }, () => "abcdefghijkmnpqrstuvwxyz23456789"[Math.floor(Math.random() * 32)]).join("");

module.exports = async (req, res) => {
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type");
  res.setHeader("Cache-Control", "no-store");
  if (req.method === "OPTIONS") return res.status(204).end();
  try {
    if (req.method === "GET") {
      const b = clean(req.query.b, 6);
      const [board, msgs] = await Promise.all([
        sb(`lab_rolling_boards?id=eq.${encodeURIComponent(b)}&select=id,to_name`).then((r) => r.json()),
        sb(`lab_rolling_msgs?board_id=eq.${encodeURIComponent(b)}&select=id,from_name,text,color,created_at&order=created_at.asc&limit=200`).then((r) => r.json()),
      ]);
      if (!Array.isArray(board) || !board.length) return res.status(404).json({ error: "없는 롤링페이퍼예요. 링크를 다시 확인해 주세요" });
      return res.status(200).json({ board: board[0], msgs });
    }
    const p = body(req);
    if (p.action === "board") {
      const to = clean(p.to, 12);
      if (!to) return res.status(400).json({ error: "받는 사람 이름을 써 주세요" });
      const id = newId();
      const w = await sb("lab_rolling_boards", { method: "POST", headers: { Prefer: "return=minimal" }, body: JSON.stringify({ id, to_name: to }) });
      if (!w.ok) throw new Error(String(w.status));
      return res.status(200).json({ id });
    }
    if (p.action === "msg") {
      const row = { board_id: clean(p.b, 6), from_name: clean(p.from, 12), text: clean(p.text, 120), color: COLORS.includes(p.color) ? p.color : "yellow" };
      if (!row.from_name || !row.text) return res.status(400).json({ error: "이름과 쪽지 내용을 써 주세요" });
      const w = await sb("lab_rolling_msgs", { method: "POST", headers: { Prefer: "return=minimal" }, body: JSON.stringify(row) });
      if (!w.ok) return res.status(w.status === 409 || w.status === 400 ? 404 : 500).json({ error: "쪽지를 붙이지 못했어요" });
      return res.status(200).json({ ok: true });
    }
    res.status(400).json({ error: "알 수 없는 요청이에요" });
  } catch (e) {
    res.status(500).json({ error: "DB와 이야기하지 못했어요" });
  }
};
