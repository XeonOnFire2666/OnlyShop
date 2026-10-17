(function () {
    const cfg = window.APP_CONFIG;
    const tg = window.Telegram && window.Telegram.WebApp;
    const initData = tg && tg.initData ? tg.initData : '';
    const useMock = cfg.mock || new URLSearchParams(location.search).has('mock') || !initData;

    async function request(method, path, body) {
        if (useMock) return window.App.mockRequest(method, path, body);

        const sep = path.includes('?') ? '&' : '?';
        const url = (cfg.apiBase || '') + '/api' + path + sep + '_t=' + Date.now();

        let res;
        try {
            res = await fetch(url, {
                method,
                cache: 'no-store',     
                headers: {
                    'Content-Type': 'application/json',
                    Accept: 'application/json',
                    Authorization: 'tma ' + initData
                },
                body: body === undefined ? undefined : JSON.stringify(body)
            });
        } catch (networkError) {
            const error = new Error(
                'Не удалось связаться с сервером. Проверьте подключение и адрес API (' + url + ').'
            );
            error.status = 0;
            error.cause = networkError;
            throw error;
        }

        const contentType = (res.headers.get('content-type') || '').toLowerCase();
        const isJson = contentType.includes('application/json') || contentType.includes('+json');

        if (!res.ok) {
            let message = '';
            let payload = null;

            if (isJson) {
                try {
                    payload = await res.json();
                    message =
                        payload.detail ||
                        payload.title ||
                        payload.message ||
                        (payload.errors && Object.values(payload.errors).flat().join('; ')) ||
                        '';
                } catch (e) {
                }
            } else {
                let text = '';
                try {
                    text = await res.text();
                } catch (e) {
                }
                const snippet = text.trim().slice(0, 180).replace(/\s+/g, ' ');

                if (res.status === 405) {
                    message =
                        'Сервер не принимает ' +
                        method +
                        ' на ' +
                        path +
                        '. Похоже, запрос уходит на фронтенд-хостинг, а не на API. Проверьте apiBase и прокси /api/*.';
                } else if (/^<!doctype|^<html/i.test(text.trim())) {
                    message =
                        'API вернул HTML вместо JSON (HTTP ' +
                        res.status +
                        '). Скорее всего, запрос попал на SPA-фолбэк. URL: ' +
                        url;
                } else if (snippet) {
                    message = 'HTTP ' + res.status + ': ' + snippet;
                } else {
                    message = 'Ошибка ' + res.status;
                }
            }

            if (!message) message = 'Ошибка ' + res.status;

            const error = new Error(message);
            error.status = res.status;
            error.payload = payload;
            throw error;
        }

        if (!isJson) {
            let text = '';
            try {
                text = await res.text();
            } catch (e) {
            }

            if (!text) return null;

            const trimmed = text.trim();
            if (/^<!doctype|^<html/i.test(trimmed)) {
                throw new Error(
                    'API вернул HTML вместо JSON (HTTP ' +
                    res.status +
                    '). Скорее всего, запрос попал на SPA-фолбэк. URL: ' +
                    url
                );
            }

            try {
                return JSON.parse(trimmed);
            } catch (e) {
                throw new Error(
                    'Ожидался JSON, получен ' +
                    (contentType || 'unknown') +
                    '. Ответ: ' +
                    trimmed.slice(0, 180)
                );
            }
        }

        const raw = await res.text();
        return raw ? JSON.parse(raw) : null;
    }

    const pick = (obj, ...keys) => {
        for (const key of keys) {
            if (obj && obj[key] !== undefined && obj[key] !== null) return obj[key];
        }
        return undefined;
    };

    const photoUrl = (path) => {
        if (!path) return '';
        if (/^(https?:|data:)/.test(path)) return path;
        if (path.startsWith('/')) {
            return (cfg.apiBase || '') + path;
        }
        return (cfg.photoBase || '') + path;
    };

    const isPresaleValue = (value) => String(value).toLowerCase() === 'presale' || value === 1;
    const STATUSES = ['Pending', 'Confirmed', 'Paid', 'Delivering', 'Completed', 'Canceled'];
    const normStatus = (status) => (typeof status === 'number' ? STATUSES[status] : status);

    function normProduct(p) {
        const variants = (p.variants || []).map((v) => ({
            id: v.id,
            size: String(v.size),
            stock: pick(v, 'stockQuantity', 'stock', 'quantity') ?? (v.isAvailable ? 1 : 0)
        }));
        const price = p.price || {};
        const isPresale = pick(p, 'isPresale') ?? isPresaleValue(p.availability);

        const photos = (p.photoUrls || p.photos || [])
            .map((x) =>
                typeof x === 'string'
                    ? { path: x, order: 0 }
                    : { path: pick(x, 'url', 'storagePath', 'path'), order: x.sortOrder || 0 }
            )
            .filter((x) => x.path)
            .sort((a, b) => a.order - b.order)
            .map((x) => photoUrl(x.path));

        return {
            id: p.id,
            name: p.name,
            brand: p.brand || '',
            description: p.description || '',
            categoryId: p.categoryId,
            priceByn: pick(p, 'priceByn') ?? pick(price, 'byn') ?? 0,
            priceRub: pick(p, 'priceRub') ?? pick(price, 'rub') ?? 0,
            photos,
            variants,
            isPresale,
            presaleClosesAt: p.presaleClosesAt || null,
            canBeOrdered:
                pick(p, 'canBeOrdered') ?? (isPresale || variants.some((v) => v.stock > 0)),
            createdAt: p.createdAt || null
        };
    }

    function normOrder(o) {
        return {
            id: o.id,
            number: pick(o, 'number', 'orderNumber'),
            status: normStatus(o.status),
            currency: String(o.currency || 'Byn').toUpperCase() === 'RUB' ? 'RUB' : 'BYN',
            total: pick(o, 'total', 'totalAmount') ?? 0,
            createdAt: o.createdAt,
            items: (o.items || []).map((i) => ({
                productName: i.productName,
                size: i.size,
                quantity: i.quantity,
                unitPrice: i.unitPrice
            }))
        };
    }

    const normUser = (u) => ({
        id: u.id,
        telegramId: u.telegramId,
        name: u.name || '',
        username: u.username || '',
        phone: u.phone || '',
        bonusBalance: Number(u.bonusBalance || 0)
    });

    const listOf = (r) => (Array.isArray(r) ? r : (r && r.items) || []);

    const localFavorites = {
        get() {
            try {
                return JSON.parse(localStorage.getItem('favorites') || '[]');
            } catch (e) {
                return [];
            }
        },
        set(ids) {
            localStorage.setItem('favorites', JSON.stringify(ids));
        }
    };

    const api = {
        useMock,
        tg,

        me: async () => normUser(await request('GET', '/me')),
        updateMe: async (body) => normUser(await request('PUT', '/me', body)),

        categories: async () =>
            listOf(await request('GET', '/catalog/categories'))
                .map((c) => ({
                    id: c.id,
                    name: c.name,
                    slug: c.slug,
                    parentId: c.parentId || null,
                    sortOrder: c.sortOrder || 0
                }))
                .sort((a, b) => a.sortOrder - b.sortOrder),

        products: async ({ search, categoryId, page, pageSize }) => {
            const q = new URLSearchParams();
            if (search) q.set('search', search);
            if (categoryId) q.set('categoryId', categoryId);
            q.set('page', page);
            q.set('pageSize', pageSize);
            const r = await request('GET', '/catalog/products?' + q.toString());
            const items = listOf(r).map(normProduct);
            const total = Array.isArray(r) ? items.length : pick(r, 'total', 'totalCount') ?? items.length;
            return { items, total };
        },

        getProduct: async (id) => normProduct(await request('GET', '/catalog/products/' + id)),

        basket: async () =>
            listOf(await request('GET', '/basket')).map((i) => ({
                productId: i.productId,
                variantId: pick(i, 'productVariantId', 'variantId'),
                quantity: i.quantity
            })),
        addToBasket: (productId, variantId, quantity) =>
            request('POST', '/basket/items', { productId, variantId, quantity }),
        setQuantity: (variantId, quantity) =>
            request('PUT', '/basket/items/' + variantId, { quantity }),
        removeFromBasket: (variantId) => request('DELETE', '/basket/items/' + variantId),
        clearBasket: () => request('DELETE', '/basket'),

        checkout: (payload) => request('POST', '/orders', payload),
        orders: async () =>
            listOf(await request('GET', '/orders?page=1&pageSize=20')).map(normOrder),
        cancelOrder: (id) => request('POST', '/orders/' + id + '/cancel'),

        favorites: {
            async list() {
                try {
                    const r = await request('GET', '/favorites');
                    const ids = listOf(r).map((x) =>
                        typeof x === 'string' ? x : pick(x, 'productId', 'id')
                    );
                    localFavorites.set(ids);
                    return ids;
                } catch (e) {
                    return localFavorites.get();
                }
            },
            async add(id) {
                localFavorites.set([id, ...localFavorites.get().filter((x) => x !== id)]);
                try {
                    await request('PUT', '/favorites/' + id);
                } catch (e) {
                    return;
                }
            },
            async remove(id) {
                localFavorites.set(localFavorites.get().filter((x) => x !== id));
                try {
                    await request('DELETE', '/favorites/' + id);
                } catch (e) {
                    return;
                }
            }
        }
    };

    const convert = (byn, rub, currency) => {
        if (currency === 'RUB') return rub;
        if (currency === 'USD') return Math.round(byn / cfg.usdRate);
        return byn;
    };

    const formatMoney = (value, currency) => {
        const n = new Intl.NumberFormat('ru-RU').format(Math.round(value));
        if (currency === 'USD') return '$' + n;
        return n + ' ' + (currency === 'RUB' ? '₽' : 'Br');
    };

    window.App.api = api;
    window.App.money = {
        price: (p, currency) => convert(p.priceByn, p.priceRub, currency),
        convert,
        format: formatMoney
    };
})();