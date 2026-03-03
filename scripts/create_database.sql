-- Создание базы данных
CREATE DATABASE stockmarket
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TEMPLATE = template0;

\c stockmarket

-- Таблица тиков
CREATE TABLE ticks (
    id BIGSERIAL PRIMARY KEY,
    ticker VARCHAR(32) NOT NULL,
    price NUMERIC(18, 8) NOT NULL,
    volume NUMERIC(18, 8) NOT NULL,
    "timestamp" TIMESTAMPTZ NOT NULL,
    source VARCHAR(64) NOT NULL
);

-- Индексы для типичных запросов
CREATE INDEX ix_ticks_ticker_timestamp ON ticks (ticker, "timestamp");
CREATE INDEX ix_ticks_timestamp ON ticks ("timestamp");
CREATE INDEX ix_ticks_source ON ticks (source);
