import os
import json
import traceback
from pathlib import Path
from graphify.extract import collect_files, extract
from graphify.build import build_from_json
from graphify.cluster import cluster, score_all
from graphify.analyze import god_nodes, surprising_connections, suggest_questions
from graphify.report import generate
from graphify.export import to_json, to_html

def main():
    print("--- Starting AST-only Graphify Pipeline ---")
    
    # 1. Read detection output
    detect_path = Path('graphify-out/.graphify_detect.json')
    if not detect_path.exists():
        print("Error: detect json not found. Run detection first.")
        return
        
    detect = json.loads(detect_path.read_text(encoding="utf-8"))
    
    # 2. Collect code files
    code_files = []
    for f in detect.get('files', {}).get('code', []):
        code_files.extend(collect_files(Path(f)) if Path(f).is_dir() else [Path(f)])
        
    print(f"Collecting files completed: {len(code_files)} code files found.")
    
    # 3. Extract AST structures
    print("Extracting AST (Abstract Syntax Tree) structures...")
    if code_files:
        try:
            result = extract(code_files)
        except Exception as e:
            print("Error in AST extraction:")
            traceback.print_exc()
            return
        
        Path('graphify-out/.graphify_ast.json').write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
        print(f"AST extraction complete: {len(result['nodes'])} nodes, {len(result['edges'])} edges")
    else:
        result = {'nodes':[],'edges':[],'input_tokens':0,'output_tokens':0}
        Path('graphify-out/.graphify_ast.json').write_text(json.dumps(result, ensure_ascii=False), encoding="utf-8")
        print('No code files found.')
        
    # 4. Save to final extract path
    print("Writing extraction to final path...")
    result['input_tokens'] = 0
    result['output_tokens'] = 0
    Path('graphify-out/.graphify_extract.json').write_text(json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
    
    # 5. Build graph, cluster, and analyze
    print("Building knowledge graph, clustering communities, and generating reports...")
    G = build_from_json(result)
    
    if G.number_of_nodes() == 0:
        print("Error: Graph is empty - no nodes extracted.")
        return
        
    communities = cluster(G)
    cohesion = score_all(G, communities)
    tokens = {'input': 0, 'output': 0}
    gods = god_nodes(G)
    surprises = surprising_connections(G, communities)
    
    # Auto-generate labels for communities based on their constituent nodes
    labels = {}
    for cid, nodes in communities.items():
        # Get up to 3 node labels in this community to formulate a name
        node_labels = [G.nodes[nid].get('label', nid) for nid in nodes[:3]]
        if len(node_labels) == 1:
            name = f"{node_labels[0]} Area"
        elif len(node_labels) >= 2:
            name = " & ".join(node_labels[:2])
        else:
            name = f"Community {cid}"
        labels[cid] = name
        
    print(f"Community labels generated: {labels}")
    
    questions = suggest_questions(G, communities, labels)
    
    report = generate(G, communities, cohesion, labels, gods, surprises, detect, tokens, 'c:/Proyectos/1.-DGMESNIE', suggested_questions=questions)
    Path('graphify-out/GRAPH_REPORT.md').write_text(report, encoding="utf-8")
    to_json(G, communities, 'graphify-out/graph.json')
    
    analysis = {
        'communities': {str(k): v for k, v in communities.items()},
        'cohesion': {str(k): v for k, v in cohesion.items()},
        'gods': gods,
        'surprises': surprises,
        'questions': questions,
    }
    Path('graphify-out/.graphify_analysis.json').write_text(json.dumps(analysis, indent=2, ensure_ascii=False), encoding="utf-8")
    
    # 6. Generate HTML
    print("Generating HTML visualization...")
    if G.number_of_nodes() > 5000:
        print(f"Graph has {G.number_of_nodes()} nodes - too large for HTML viz.")
    else:
        to_html(G, communities, 'graphify-out/graph.html', community_labels=labels)
        print("graphify-out/graph.html generated successfully!")
        
    print(f"Pipeline complete! Graph: {G.number_of_nodes()} nodes, {G.number_of_edges()} edges, {len(communities)} communities.")

if __name__ == '__main__':
    main()
