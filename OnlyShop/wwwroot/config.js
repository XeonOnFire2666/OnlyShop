window.App = window.App || {};

window.APP_CONFIG = {
  apiBase: '',
  photoBase: '/media/',
  managerUsername: 'sneakersland_manager',
  usdRate: 3.2,
  mock: false,
  shopAddress: 'Притыцкого 29, Тивали, гал. №101',
  deliveryMethods: [
    { id: 'Courier', label: 'Курьер', wait: '1–3 дня', needsAddress: true },
    { id: 'Pickup', label: 'Самовывоз', wait: 'по договорённости', needsAddress: false }
  ],
  paymentMethods: [
    { id: 'Cash', label: 'Наличными' },
    { id: 'Card', label: 'Картой' }
  ],
  deliveryInfo: [
    'Доставка по Беларуси — 1–3 дня после подтверждения заказа.',
    'Самовывоз из магазина: Притыцкого 29, Тивали, гал. №101.',
    'Менеджер свяжется с вами в Telegram после оформления.'
  ]
};
