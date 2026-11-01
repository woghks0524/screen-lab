-- API 이해하기: 진짜 앱 5개 중 DB를 쓰는 2개(밸런스 게임, 롤링페이퍼)의 표
-- booksuda 프로젝트를 같이 쓰니 표 이름은 모두 lab_ 로 시작해요.
-- 브라우저는 공개 키(SB_ANON_KEY)로 서버 함수(api/balance.js, api/rolling.js)를 거쳐 읽고 써요.

-- 밸런스 게임: 한 번 고를 때마다 한 줄
create table if not exists lab_balance (
  id bigint generated always as identity primary key,
  question_id int not null check (question_id between 1 and 50),
  pick text not null check (pick in ('a', 'b')),
  created_at timestamptz not null default now()
);

-- 문제마다 A·B 몇 명인지 세어 둔 보기(view)
create or replace view lab_balance_counts as
  select question_id,
         count(*) filter (where pick = 'a')::int as a,
         count(*) filter (where pick = 'b')::int as b
  from lab_balance group by question_id;

-- 롤링페이퍼: 판(누구에게) 하나 + 쪽지 여러 장
create table if not exists lab_rolling_boards (
  id text primary key check (id ~ '^[a-z0-9]{6}$'),
  to_name text not null check (char_length(to_name) between 1 and 12),
  created_at timestamptz not null default now()
);

create table if not exists lab_rolling_msgs (
  id bigint generated always as identity primary key,
  board_id text not null references lab_rolling_boards(id) on delete cascade,
  from_name text not null check (char_length(from_name) between 1 and 12),
  text text not null check (char_length(text) between 1 and 120),
  color text not null check (color in ('yellow', 'pink', 'mint', 'sky', 'lilac')),
  created_at timestamptz not null default now()
);
create index if not exists lab_rolling_msgs_board on lab_rolling_msgs(board_id, created_at);

-- 권한 규칙(RLS): 누구나 읽고 새로 쓰기만. 고치기·지우기는 없어요.
alter table lab_balance enable row level security;
alter table lab_rolling_boards enable row level security;
alter table lab_rolling_msgs enable row level security;

drop policy if exists "lab 투표 읽기" on lab_balance;
create policy "lab 투표 읽기" on lab_balance for select using (true);
drop policy if exists "lab 투표 하기" on lab_balance;
create policy "lab 투표 하기" on lab_balance for insert with check (true);

drop policy if exists "lab 판 읽기" on lab_rolling_boards;
create policy "lab 판 읽기" on lab_rolling_boards for select using (true);
drop policy if exists "lab 판 만들기" on lab_rolling_boards;
create policy "lab 판 만들기" on lab_rolling_boards for insert with check (true);

drop policy if exists "lab 쪽지 읽기" on lab_rolling_msgs;
create policy "lab 쪽지 읽기" on lab_rolling_msgs for select using (true);
drop policy if exists "lab 쪽지 쓰기" on lab_rolling_msgs;
create policy "lab 쪽지 쓰기" on lab_rolling_msgs for insert with check (true);

-- 보기(view)는 만든 사람 권한으로 읽혀서, 공개 키로는 숫자만 볼 수 있게 읽기만 열어 줘요
grant select on lab_balance_counts to anon;
