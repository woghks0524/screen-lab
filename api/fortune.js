// AI 오늘의 운세 · 이름 삼행시 API. 브라우저 → 이 서버 함수 → OpenAI.
// OpenAI 키는 Vercel 환경변수(OPENAI_API_KEY)에만 있어서, 화면 코드나 F12에는 나오지 않아요.
// POST { name, kind: "fortune" | "acrostic" } → { lines: ["…", "…"] }
const MODEL = process.env.OPENAI_MODEL || "gpt-5.4-mini";

const PROMPT = {
  fortune: (name, today) =>
    `${today}, "${name}" 님의 오늘의 운세를 재미있게 지어 줘. 선생님 연수에서 웃자고 보는 거야.
딱 4줄만 써. 1줄: "총운: …", 2줄: "행운의 물건: …", 3줄: "조심할 것: …"(웃긴 것으로), 4줄: "한마디: …".
한 줄에 35자 이내, 다정하고 유쾌하게, 나쁜 예언·건강·돈 이야기는 쓰지 마. 줄 앞에 번호나 기호는 붙이지 마.`,
  acrostic: (name) =>
    `"${name}" 이름으로 삼행시(글자 수만큼 줄)를 지어 줘. 각 줄은 이름의 글자를 하나씩 차례로 시작 글자로 써.
형식은 "글자: 문장" 한 줄씩. 선생님을 기분 좋게 하는 칭찬이나 반전 유머로, 한 줄에 30자 이내. 다른 말은 쓰지 마.`,
};

const body = (req) => (typeof req.body === "string" ? JSON.parse(req.body || "{}") : req.body || {});

module.exports = async (req, res) => {
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type");
  res.setHeader("Cache-Control", "no-store");
  if (req.method === "OPTIONS") return res.status(204).end();
  if (req.method !== "POST") return res.status(405).json({ error: "POST로 불러 주세요" });
  if (!process.env.OPENAI_API_KEY) return res.status(503).json({ error: "서버에 AI 키가 아직 없어요" });

  const { name, kind } = body(req);
  const who = String(name || "").replace(/\s/g, "").slice(0, 6);
  if (!who || !PROMPT[kind]) return res.status(400).json({ error: "이름(6글자까지)을 써 주세요" });
  const today = new Date().toLocaleDateString("ko-KR", { timeZone: "Asia/Seoul", month: "long", day: "numeric", weekday: "long" });

  try {
    const r = await fetch("https://api.openai.com/v1/chat/completions", {
      method: "POST",
      headers: { Authorization: `Bearer ${process.env.OPENAI_API_KEY}`, "Content-Type": "application/json" },
      body: JSON.stringify({ model: MODEL, messages: [{ role: "user", content: PROMPT[kind](who, today) }] }),
    });
    const j = await r.json();
    if (!r.ok) throw new Error(j.error?.message || String(r.status));
    const lines = String(j.choices?.[0]?.message?.content || "").split("\n").map((s) => s.trim()).filter(Boolean).slice(0, 6);
    res.status(200).json({ lines, model: MODEL });
  } catch (e) {
    res.status(500).json({ error: "AI가 잠깐 쉬고 있어요. 다시 눌러 주세요" });
  }
};
