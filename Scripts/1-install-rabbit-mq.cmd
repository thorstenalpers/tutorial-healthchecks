helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
helm install -f rabbitmq-values.yaml rabbitmq bitnami/rabbitmq