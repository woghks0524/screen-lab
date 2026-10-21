// 수도 퀴즈 문제를 DB(Supabase lab_quiz 표)에서 꺼내 주는 API. 키는 Vercel 환경변수에만 있어요.
module.exports = async (req, res) => {
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Cache-Control", "no-store");
  try {
    const r = await fetch(`${process.env.SB_URL}/rest/v1/lab_quiz?select=country,capital`, {
      headers: { apikey: process.env.SB_ANON_KEY, Authorization: `Bearer ${process.env.SB_ANON_KEY}` },
    });
    if (!r.ok) throw new Error(String(r.status));
    res.status(200).json(await r.json());
  } catch (e) {
    res.status(500).json({ error: "DB에서 불러오지 못했어요" });
  }
};
