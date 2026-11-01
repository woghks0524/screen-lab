// 밸런스 게임 투표 API. DB(Supabase lab_balance 표)에 한 표씩 쌓고, 문제별 A·B 수를 돌려줘요.
// GET: [{ question_id, a, b }, …]   POST { q: 문제 번호, pick: "a" | "b" }: 한 표 넣고 같은 목록을 돌려줘요
const sb = (path, init = {}) =>
  fetch(`${process.env.SB_URL}/rest/v1/${path}`, {
    ...init,
    headers: { apikey: process.env.SB_ANON_KEY, Authorization: `Bearer ${process.env.SB_ANON_KEY}`, "Content-Type": "application/json", ...init.headers },
  });

const body = (req) => (typeof req.body === "string" ? JSON.parse(req.body || "{}") : req.body || {});

module.exports = async (req, res) => {
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type");
  res.setHeader("Cache-Control", "no-store");
  if (req.method === "OPTIONS") return res.status(204).end();
  try {
    if (req.method === "POST") {
      const { q, pick } = body(req);
      if (!Number.isInteger(q) || q < 1 || q > 50 || !["a", "b"].includes(pick)) return res.status(400).json({ error: "문제 번호와 a·b 중 하나가 필요해요" });
      const w = await sb("lab_balance", { method: "POST", headers: { Prefer: "return=minimal" }, body: JSON.stringify({ question_id: q, pick }) });
      if (!w.ok) throw new Error(String(w.status));
    }
    const r = await sb("lab_balance_counts?select=question_id,a,b");
    if (!r.ok) throw new Error(String(r.status));
    res.status(200).json(await r.json());
  } catch (e) {
    res.status(500).json({ error: "DB와 이야기하지 못했어요" });
  }
};
