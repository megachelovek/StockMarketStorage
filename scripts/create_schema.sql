-- Схема для базы stockmarket (БД должна уже существовать)
-- Выполнять: psql -U postgres -d stockmarket -f create_schema.sql

-- Таблица тиков
CREATE TABLE IF NOT EXISTS ticks (
    id BIGSERIAL PRIMARY KEY,
    ticker VARCHAR(32) NOT NULL,
    price NUMERIC(18, 8) NOT NULL,
    volume NUMERIC(18, 8) NOT NULL,
    "timestamp" TIMESTAMPTZ NOT NULL,
    source VARCHAR(64) NOT NULL
);

-- Индексы для типичных запросов
CREATE INDEX IF NOT EXISTS ix_ticks_ticker_timestamp ON ticks (ticker, "timestamp");
CREATE INDEX IF NOT EXISTS ix_ticks_timestamp ON ticks ("timestamp");
CREATE INDEX IF NOT EXISTS ix_ticks_source ON ticks (source);
