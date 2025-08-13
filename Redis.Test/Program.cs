using StackExchange.Redis;
using System.Diagnostics;

Console.WriteLine("Redis 접속 테스트 시작");

var redisConnectionString = "localhost:6379";

try
{
    // 연결 유지 (using 제거)
    var redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
    Console.WriteLine("Redis 연결 성공!");

    // DB 10 선택 (기본이 0이지만 명시적으로 지정)
    var db = redis.GetDatabase(10);

    // 구독자 설정
    var sub = redis.GetSubscriber();
    const string channelName = "AAAA";

    await sub.SubscribeAsync(channelName, (channel, message) =>
    {
        Console.WriteLine($"[수신] 채널:{channel} 메시지:{message}");
    });
    Console.WriteLine($"채널 '{channelName}' 구독 시작");

    // 핑 모니터링 Task
    var pingCts = new CancellationTokenSource();
    var pingTask = Task.Run(async () =>
    {
        while (!pingCts.Token.IsCancellationRequested)
        {
            try
            {
                var pingResultAsync = await db.PingAsync();
                Debug.WriteLine($"PING {pingResultAsync.TotalMilliseconds} ms");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ping 실패: " + ex.Message);
            }
            await Task.Delay(2_000, pingCts.Token); // 2초 간격
        }
    }, pingCts.Token);

    Console.WriteLine("메시지를 입력하면 DB0에 저장 후 채널로 Publish 됩니다. 종료: exit");

    while (true)
    {
        string? line = Console.ReadLine();

        if (line is null) continue; // null (Ctrl+Z 등) 무시
        if (string.Equals(line, "exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }
        if (string.IsNullOrWhiteSpace(line))
            continue;

        try
        {
            // Key 생성 (UTC 타임스탬프 기반)
            //var key = $"msg:{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            var key = $"KR34";
            await db.StringSetAsync(key, line);
            Console.WriteLine($"[저장] {key} => {line}");

            // Publish
            long receivers = await sub.PublishAsync(channelName, line);
            Console.WriteLine($"[전송] 채널:{channelName} 수신자:{receivers} 메시지:{line}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("처리 오류: " + ex.Message);
        }
    }

    // 정리
    pingCts.Cancel();
    try { await pingTask; } catch { /* ignore */ }
    await sub.UnsubscribeAllAsync();
    await redis.CloseAsync();
    redis.Dispose();
    Console.WriteLine("종료");
}
catch (Exception ex)
{
    Console.WriteLine($"Redis 연결 실패: {ex.Message}");
}
