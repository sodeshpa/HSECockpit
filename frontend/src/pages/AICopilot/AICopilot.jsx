import { useState, useRef, useEffect } from "react";
import { Send, Bot, User, BookOpen, Lightbulb, AlertCircle } from "lucide-react";
import { queryCopilot } from "../../api/copilot";

function ChatMessage({ message }) {
  if (message.role === "user") {
    return (
      <div className="flex items-start gap-3 justify-end">
        <div className="rounded-lg bg-blue-600 px-4 py-3 text-sm text-white max-w-[70%]">
          {message.content}
        </div>
        <div className="rounded-full bg-blue-100 p-2 shrink-0">
          <User className="h-4 w-4 text-blue-600" aria-hidden="true" />
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-start gap-3">
      <div className="rounded-full bg-green-100 p-2 shrink-0">
        <Bot className="h-4 w-4 text-green-600" aria-hidden="true" />
      </div>
      <div className="max-w-[80%] space-y-3">
        {/* Answer */}
        <div className="rounded-lg bg-white border border-gray-200 px-4 py-3 shadow-sm">
          <div className="flex items-center gap-2 mb-2">
            <span className="inline-flex items-center rounded bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-800">Observed</span>
          </div>
          <div className="text-sm text-gray-800 whitespace-pre-wrap prose prose-sm max-w-none" dangerouslySetInnerHTML={{ __html: message.content.answer?.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>').replace(/\n/g, '<br/>') || message.content.answer }} />
        </div>

        {/* Recommended Actions */}
        {message.content.recommendedActions && message.content.recommendedActions.length > 0 && (
          <div className="rounded-lg bg-amber-50 border border-amber-200 px-4 py-3">
            <div className="flex items-center gap-2 mb-2">
              <Lightbulb className="h-4 w-4 text-amber-600" aria-hidden="true" />
              <span className="inline-flex items-center rounded bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-800">Recommended</span>
            </div>
            <ul className="space-y-2">
              {message.content.recommendedActions.map((action, i) => (
                <li key={action.ruleId || i} className="text-sm text-gray-700">
                  <span className="font-medium">{action.label}:</span> {action.action}
                  <p className="text-xs text-gray-500 mt-0.5">Triggered by: {action.triggeringCondition}</p>
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Citations */}
        {message.content.citations && message.content.citations.length > 0 && (
          <details className="rounded-lg bg-gray-50 border border-gray-200 px-4 py-3">
            <summary className="flex items-center gap-2 cursor-pointer text-sm text-gray-600">
              <BookOpen className="h-4 w-4" aria-hidden="true" />
              {message.content.citations.length} source record{message.content.citations.length > 1 ? "s" : ""} cited
            </summary>
            <ul className="mt-2 space-y-1">
              {message.content.citations.map((c, i) => (
                <li key={c.recordId || i} className="text-xs text-gray-600 pl-6">
                  <span className="font-medium">{c.recordType}</span>: {c.textContent}
                  <span className="text-gray-400 ml-2">({(c.relevanceScore * 100).toFixed(0)}% match)</span>
                </li>
              ))}
            </ul>
          </details>
        )}

        {/* Data Scope Warning */}
        {message.content.dataScope === "partial" && (
          <div className="flex items-center gap-2 text-xs text-yellow-700 bg-yellow-50 px-3 py-2 rounded">
            <AlertCircle className="h-3 w-3" aria-hidden="true" />
            Some records have data quality flags — results may be incomplete.
          </div>
        )}
        {message.content.dataScope === "out_of_scope" && (
          <div className="flex items-center gap-2 text-xs text-gray-600 bg-gray-100 px-3 py-2 rounded">
            <AlertCircle className="h-3 w-3" aria-hidden="true" />
            This question is outside the current data scope.
          </div>
        )}
      </div>
    </div>
  );
}

export function AICopilot() {
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef(null);
  const inputRef = useRef(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!input.trim() || isLoading) return;

    const userMessage = { role: "user", content: input.trim() };
    setMessages(prev => [...prev, userMessage]);
    setInput("");
    setIsLoading(true);

    try {
      const response = await queryCopilot(userMessage.content);
      setMessages(prev => [...prev, { role: "assistant", content: response }]);
    } catch {
      setMessages(prev => [...prev, { role: "assistant", content: { answer: "Sorry, I was unable to process your question. Please try again.", citations: [], recommendedActions: [], dataScope: "error" } }]);
    } finally {
      setIsLoading(false);
      inputRef.current?.focus();
    }
  };

  return (
    <section className="flex flex-col h-full max-h-[calc(100vh-2rem)]" aria-label="AI Copilot">
      {/* Header */}
      <header className="border-b border-gray-200 bg-white px-6 py-4">
        <h1 className="text-2xl font-bold text-gray-900">AI HSE Copilot</h1>
        <p className="mt-1 text-sm text-gray-600">
          Ask questions about barriers, incidents, risks, and maintenance records.
        </p>
      </header>

      {/* Messages Area */}
      <div className="flex-1 overflow-y-auto px-6 py-4 space-y-6">
        {messages.length === 0 && (
          <div className="flex flex-col items-center justify-center h-full text-center text-gray-400 py-12">
            <Bot className="h-12 w-12 mb-4" />
            <p className="text-lg font-medium text-gray-600">How can I help you today?</p>
            <p className="text-sm mt-2 max-w-md">Try asking about barrier status, incidents, risk trends, or maintenance records across your sites.</p>
            <div className="mt-6 grid grid-cols-1 sm:grid-cols-2 gap-3 max-w-lg">
              <button onClick={() => setInput("Show barriers at risk across all sites")} className="text-left rounded-lg border border-gray-200 bg-white px-4 py-3 text-sm text-gray-700 hover:bg-gray-50 transition-colors">
                Show barriers at risk across all sites
              </button>
              <button onClick={() => setInput("What incidents occurred this month?")} className="text-left rounded-lg border border-gray-200 bg-white px-4 py-3 text-sm text-gray-700 hover:bg-gray-50 transition-colors">
                What incidents occurred this month?
              </button>
              <button onClick={() => setInput("Which sites have the highest risk score?")} className="text-left rounded-lg border border-gray-200 bg-white px-4 py-3 text-sm text-gray-700 hover:bg-gray-50 transition-colors">
                Which sites have the highest risk score?
              </button>
              <button onClick={() => setInput("Are there any overdue maintenance items?")} className="text-left rounded-lg border border-gray-200 bg-white px-4 py-3 text-sm text-gray-700 hover:bg-gray-50 transition-colors">
                Are there any overdue maintenance items?
              </button>
            </div>
          </div>
        )}
        {messages.map((msg, i) => (
          <ChatMessage key={i} message={msg} />
        ))}
        {isLoading && (
          <div className="flex items-start gap-3">
            <div className="rounded-full bg-green-100 p-2">
              <Bot className="h-4 w-4 text-green-600 animate-pulse" aria-hidden="true" />
            </div>
            <div className="rounded-lg bg-white border border-gray-200 px-4 py-3 shadow-sm">
              <p className="text-sm text-gray-500 animate-pulse">Analyzing HSE data...</p>
            </div>
          </div>
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* Input Area */}
      <form onSubmit={handleSubmit} className="border-t border-gray-200 bg-white px-6 py-4">
        <div className="flex items-center gap-3">
          <label htmlFor="copilot-input" className="sr-only">Ask a question about HSE data</label>
          <input
            ref={inputRef}
            id="copilot-input"
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Ask about barriers, incidents, risks..."
            className="flex-1 rounded-lg border border-gray-300 px-4 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            disabled={isLoading}
            autoComplete="off"
          />
          <button
            type="submit"
            disabled={!input.trim() || isLoading}
            className="rounded-lg bg-blue-600 px-4 py-2.5 text-white hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            aria-label="Send question"
          >
            <Send className="h-4 w-4" />
          </button>
        </div>
      </form>
    </section>
  );
}
