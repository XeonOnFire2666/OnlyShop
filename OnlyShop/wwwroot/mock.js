(function () {
  const wait = (value) => new Promise((resolve) => setTimeout(() => resolve(structuredClone(value)), 160));

  const art = (from, to, label) =>
    'data:image/svg+xml;utf8,' +
    encodeURIComponent(
      `<svg xmlns="http://www.w3.org/2000/svg" width="600" height="750"><defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${from}"/><stop offset="1" stop-color="${to}"/></linearGradient></defs><rect width="600" height="750" fill="url(#g)"/><text x="300" y="390" font-family="Oswald,Arial,sans-serif" font-size="46" fill="rgba(255,255,255,.88)" text-anchor="middle">${label}</text></svg>`
    );

  const categories = [
    { id: 'c-shoes', name: 'Обувь', slug: 'shoes', parentId: null, sortOrder: 1 },
    { id: 'c-sneakers', name: 'Кроссовки', slug: 'sneakers', parentId: 'c-shoes', sortOrder: 1 },
    { id: 'c-jackets', name: 'Куртки', slug: 'jackets', parentId: null, sortOrder: 2 },
    { id: 'c-hoodies', name: 'Кофты', slug: 'hoodies', parentId: null, sortOrder: 3 }
  ];

  const product = (id, name, brand, categoryId, byn, rub, colors, sizes, description, extra) => ({
    id,
    name,
    brand,
    description,
    categoryId,
    price: { byn, rub },
    availability: 'InStock',
    status: 'Published',
    photos: colors
      ? [
          { storagePath: art(colors[0], colors[1], name), sortOrder: 0 },
          { storagePath: art(colors[1], colors[0], brand), sortOrder: 1 },
          { storagePath: art('#2b2b2b', colors[0], name), sortOrder: 2 }
        ]
      : [],
    variants: sizes.map(([size, stock]) => ({ id: `${id}-${size}`, size, stockQuantity: stock })),
    createdAt: new Date(Date.now() - Number(id.slice(1)) * 86400000).toISOString(),
    ...extra
  });

  const products = [
    product('p1', 'Maison Margiela Evolution Low', 'Maison Margiela', 'c-sneakers', 1000, 28000, ['#d8d2c4', '#8f877a'], [['41', 1], ['42', 3], ['43', 0], ['44', 1], ['45', 2]], 'тестовый'),
    product('p2', 'Nike Air Max 90', 'Nike', 'c-sneakers', 180, 5000, ['#5f6b7a', '#2c3440'], [['40', 2], ['41', 1], ['42', 4], ['43', 1], ['44', 0]], ''),
    product('p3', 'Louis Vuitton denim trainer sneakers', 'Louis Vuitton', 'c-sneakers', 1800, 50000, ['#3c6e9c', '#1d3b57'], [['42', 1]], 'Джинсовый трейнер, оригинальная коробка.'),
    product('p4', 'Armani puffer', 'Armani', 'c-jackets', 600, 16500, null, [['S', 1], ['M', 2]], ''),
    product('p5', 'Louis Vuitton monogram hoodie', 'Louis Vuitton', 'c-hoodies', 950, 26500, ['#c8b79c', '#8a7658'], [['S', 1], ['M', 1], ['L', 0]], 'Бежевый худи с монограммой.'),
    product('p6', 'Golden Goose Super-Star', 'Golden Goose', 'c-sneakers', 780, 21500, ['#e9e6e1', '#9b968e'], [['39', 5], ['40', 5], ['41', 5], ['42', 5]], 'Предзаказ из Италии.', {
      availability: 'Presale',
      presaleClosesAt: new Date(Date.now() + 9 * 86400000).toISOString()
    }),
    product('p7', 'New Balance 9060', 'New Balance', 'c-sneakers', 320, 8900, ['#9aa3a8', '#4b555c'], [['41', 2], ['42', 2], ['43', 1]], ''),
    product('p8', 'Carhartt WIP Detroit jacket', 'Carhartt', 'c-jackets', 410, 11300, ['#8b6a3e', '#4a361d'], [['M', 2], ['L', 1], ['XL', 0]], 'Классическая рабочая куртка.')
  ].map((p) => ({
    ...p,
    canBeOrdered: p.availability === 'Presale' || p.variants.some((v) => v.stockQuantity > 0)
  }));

  const state = {
    user: { id: 'u1', telegramId: 482914821, name: 'Алексей', username: 'liknine', phone: null, bonusBalance: 120 },
    favorites: ['p3'],
    basket: [],
    orders: []
  };

  const descendants = (id) => [id, ...categories.filter((c) => c.parentId === id).map((c) => c.id)];
  const findProduct = (id) => products.find((p) => p.id === id);

  window.App.mockRequest = async function (method, path, body) {
    const url = new URL(path, 'http://mock');
    const p = url.pathname;
    let m;

    if (method === 'GET' && p === '/me') return wait(state.user);

    if (method === 'PUT' && p === '/me') {
      state.user = { ...state.user, name: body.name || state.user.name, phone: body.phone ?? state.user.phone };
      return wait(state.user);
    }

    if (method === 'GET' && p === '/catalog/categories') return wait(categories);

    if (method === 'GET' && p === '/catalog/products') {
      const search = (url.searchParams.get('search') || '').toLowerCase();
      const categoryId = url.searchParams.get('categoryId');
      const page = Number(url.searchParams.get('page') || 1);
      const pageSize = Number(url.searchParams.get('pageSize') || 20);
      const filtered = products
        .filter((x) => !search || (x.name + ' ' + x.brand).toLowerCase().includes(search))
        .filter((x) => !categoryId || descendants(categoryId).includes(x.categoryId))
        .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
      return wait({ items: filtered.slice((page - 1) * pageSize, page * pageSize), total: filtered.length });
    }

    if (method === 'GET' && (m = p.match(/^\/catalog\/products\/(.+)$/))) {
      const found = findProduct(m[1]);
      if (!found) throw Object.assign(new Error('Товар не найден'), { status: 404 });
      return wait(found);
    }

    if (method === 'GET' && p === '/favorites') return wait(state.favorites);

    if (method === 'PUT' && (m = p.match(/^\/favorites\/(.+)$/))) {
      if (!state.favorites.includes(m[1])) state.favorites.unshift(m[1]);
      return wait(null);
    }

    if (method === 'DELETE' && (m = p.match(/^\/favorites\/(.+)$/))) {
      state.favorites = state.favorites.filter((id) => id !== m[1]);
      return wait(null);
    }

    if (method === 'GET' && p === '/basket') {
      return wait({ items: state.basket.map((i) => ({ productId: i.productId, productVariantId: i.variantId, quantity: i.quantity })) });
    }

    if (method === 'POST' && p === '/basket/items') {
      const existing = state.basket.find((i) => i.variantId === body.variantId);
      if (existing) existing.quantity += body.quantity;
      else state.basket.push({ productId: body.productId, variantId: body.variantId, quantity: body.quantity });
      return wait(null);
    }

    if (method === 'PUT' && (m = p.match(/^\/basket\/items\/(.+)$/))) {
      const item = state.basket.find((i) => i.variantId === m[1]);
      if (item) item.quantity = body.quantity;
      return wait(null);
    }

    if (method === 'DELETE' && (m = p.match(/^\/basket\/items\/(.+)$/))) {
      state.basket = state.basket.filter((i) => i.variantId !== m[1]);
      return wait(null);
    }

    if (method === 'DELETE' && p === '/basket') {
      state.basket = [];
      return wait(null);
    }

    if (method === 'POST' && p === '/orders') {
      if (!state.basket.length) throw new Error('Корзина пуста');
      const items = state.basket.map((i) => {
        const prod = findProduct(i.productId);
        const variant = prod.variants.find((v) => v.id === i.variantId);
        return {
          productName: prod.name,
          size: variant.size,
          quantity: i.quantity,
          unitPrice: body.currency === 'Rub' ? prod.price.rub : prod.price.byn
        };
      });
      const sum = items.reduce((acc, i) => acc + i.unitPrice * i.quantity, 0);
      const bonuses = Math.min(body.bonusesToSpend || 0, state.user.bonusBalance);
      const order = {
        id: 'o' + (state.orders.length + 31),
        number: state.orders.length + 31,
        status: 'Pending',
        currency: body.currency,
        total: sum - bonuses,
        createdAt: new Date().toISOString(),
        items
      };
      state.user.bonusBalance -= bonuses;
      state.orders.unshift(order);
      state.basket = [];
      return wait(order);
    }

    if (method === 'GET' && p === '/orders') return wait({ items: state.orders, total: state.orders.length });

    if (method === 'POST' && (m = p.match(/^\/orders\/(.+)\/cancel$/))) {
      const order = state.orders.find((o) => o.id === m[1]);
      if (order) order.status = 'Canceled';
      return wait(order);
    }

    throw new Error('Mock: ' + method + ' ' + path);
  };
})();
