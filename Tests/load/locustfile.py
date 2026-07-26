import random
from uuid import uuid4

from locust import HttpUser, between, task


class TaskApiUser(HttpUser):
    wait_time = between(0.5, 1.5)

    @task
    def create_task(self):
        task_type = random.choice([0, 1])

        payload = {
            "type": task_type,
            "data": f"Teste de carga {uuid4()}"
        }

        with self.client.post(
            "/api/tasks",
            json=payload,
            name="POST /api/tasks",
            catch_response=True
        ) as response:
            if response.status_code in (200, 201, 202):
                response.success()
            else:
                response.failure(
                    f"Status inesperado: {response.status_code} - "
                    f"{response.text[:200]}"
                )