
WT.exe  -p "Command Prompt"  kubectl port-forward svc/rabbitmq 5672:5672 ;split-pane  -p "Command Prompt"  kubectl port-forward svc/rabbitmq 15672:15672

