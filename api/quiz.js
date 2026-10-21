// 부를 때마다 새 계산 문제를 만들어 JSON으로 돌려주는 API
module.exports = (req, res) => {
  const r = (a, b) => a + Math.floor(Math.random() * (b - a + 1));
  const kind = r(0, 2);
  let q, answer;
  if (kind === 0) { const a = r(12, 89), b = r(12, 89); q = `${a} + ${b} = ?`; answer = a + b; }
  else if (kind === 1) { const a = r(50, 99), b = r(11, 49); q = `${a} - ${b} = ?`; answer = a - b; }
  else { const a = r(2, 9), b = r(2, 9); q = `${a} × ${b} = ?`; answer = a * b; }
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Cache-Control", "no-store");
  res.status(200).json({ q, answer: String(answer), made_at: new Date().toISOString() });
};
