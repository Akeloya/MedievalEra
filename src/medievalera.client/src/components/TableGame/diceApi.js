class DiceApiService {
    constructor() {
        this.baseUrl = '/api/';
    }

    // Базовый метод для fetch запросов
    async request(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        console.log(`Making API request to: ${url} with options:`, options);
      const defaultOptions = {
          method: 'GET',
            headers: {
                'Content-Type': "application/json"
            }
        };

        const fetchOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers,
            },
        };

        try {
          const response = await fetch(url, defaultOptions);

            // Проверяем статус ответа
            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.message || `HTTP error! status: ${response.status}`);
            }
          
            // Для пустых ответов (например, 204 No Content)
            if (response.status === 204) {
                return null;
            }

            return await response.json();
        } catch (error) {
            console.error(`API Request failed: ${url}`, error);
            throw error;
        }
    }

    // Получить стартовый набор кубиков
    async getStarterKit() {
        try {
          const data = await this.request('dice/starterkit');
            return data;
        } catch (error) {
            console.error("Failed to fetch starter kit:", error);
            throw new Error("Не удалось получить стартовый набор кубиков");
        }
    }

    // Получить кубик по типу
    async getDiceByType(type) {
        // Проверяем, что тип передан и корректен
        if (type === undefined || type === null || type === 0) {
            throw new Error("Не указан тип кубика");
        }

        try {
            // Используем URLSearchParams для правильного формирования query параметров
            const params = new URLSearchParams({
                type: type.toString()
            });

            const data = await this.request(`dice/get?${params}`);
            return data;
        } catch (error) {
            console.error(`Failed to fetch dice type ${type}:`, error);
            throw new Error(`Не удалось получить кубик типа ${type}`);
        }
    }

    // Дополнительный метод для проверки доступности API
    async checkHealth() {
        try {
            const response = await fetch(`${this.baseUrl}`, {
                method: "HEAD",
                credentials: "same-origin"
            });
            return response.ok;
        } catch (error) {
            console.error("API health check failed:", error);
            return false;
        }
    }
}

// Создаем и экспортируем единственный экземпляр
const diceApiService = new DiceApiService();
export default diceApiService;
